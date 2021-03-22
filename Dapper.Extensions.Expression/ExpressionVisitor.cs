using Dapper.Extensions.Expression.Adapters;
using Dapper.Extensions.Expression.MethodCalls;
using Dapper.Extensions.Expression.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Dapper.Extensions.Expression
{
    internal class ExpressionVisitor
    {
        /// <summary>
        /// sql适配器
        /// </summary>
        private readonly ISqlAdapter _adapter;

        /// <summary>
        /// 拆分的表达式列表
        /// </summary>
        private readonly IList<System.Linq.Expressions.Expression> _nodeExpressions;

        /// <summary>
        /// 拆分的表达式列表连接类型
        /// </summary>
        private readonly IList<ExpressionType> _nodeTypes;

        /// <summary>
        /// 参数序号
        /// </summary>
        private int _paramIndex = -1;

        /// <summary>
        /// 当前参数名称
        /// </summary>
        private string CurrentParameterName => $"@p_{_paramIndex}";

        public ExpressionVisitor(ISqlAdapter sqlAdapter)
        {
            _nodeExpressions = new List<System.Linq.Expressions.Expression>();
            _nodeTypes = new List<ExpressionType>();
            _adapter = sqlAdapter;
            Initialize();
        }

        internal void AddParameter(StringBuilder sqlBuilder, DynamicParameters parameters, object value)
        {
            Interlocked.Increment(ref _paramIndex);
            sqlBuilder.Append(CurrentParameterName);
            parameters.Add(CurrentParameterName, value);
        }

        private void AddParameter(StringBuilder sqlBuilder, DynamicParameters parameters, string columnName, object value)
        {
            Interlocked.Increment(ref _paramIndex);
            sqlBuilder.AppendFormat("{0}={1}", columnName, CurrentParameterName);
            parameters.Add(CurrentParameterName, value);
        }

        private void Initialize()
        {
            if (VisitorProvider.ContainsKey())
            {
                return;
            }
            VisitorProvider.AddVisitor(ExpressionType.Lambda, VisitLambda);
            VisitorProvider.AddVisitor(ExpressionType.Equal, VisitBinary);
            VisitorProvider.AddVisitor(ExpressionType.Add, VisitBinary);
            VisitorProvider.AddVisitor(ExpressionType.AndAlso, VisitBinary);
            VisitorProvider.AddVisitor(ExpressionType.GreaterThan, VisitBinary);
            VisitorProvider.AddVisitor(ExpressionType.GreaterThanOrEqual, VisitBinary);
            VisitorProvider.AddVisitor(ExpressionType.LessThan, VisitBinary);
            VisitorProvider.AddVisitor(ExpressionType.LessThanOrEqual, VisitBinary);
            VisitorProvider.AddVisitor(ExpressionType.Or, VisitBinary);
            VisitorProvider.AddVisitor(ExpressionType.OrElse, VisitBinary);
            VisitorProvider.AddVisitor(ExpressionType.NotEqual, VisitBinary);
            VisitorProvider.AddVisitor(ExpressionType.MemberAccess, VisitMember);
            VisitorProvider.AddVisitor(ExpressionType.Constant, VisitConstant);
            VisitorProvider.AddVisitor(ExpressionType.Call, VisitMethodCall);
            VisitorProvider.AddVisitor(ExpressionType.Not, VisitUnaryNot);
            VisitorProvider.AddVisitor(ExpressionType.Convert, VisitUnaryConvert);
            VisitorProvider.AddVisitor(ExpressionType.New, VisitNew);
            VisitorProvider.AddVisitor(ExpressionType.MemberInit, VisitMemberInit);
            VisitorProvider.AddVisitor(ExpressionType.NewArrayInit, VisitNewArray);
            VisitorProvider.AddVisitor(ExpressionType.ListInit, VisitListInit);
        }

        private void VisitLambda(System.Linq.Expressions.Expression e, StringBuilder sqlBuilder, DynamicParameters parameters)
        {
            Debug.WriteLine(nameof(VisitLambda) + sqlBuilder);

            if (!(e is LambdaExpression lambda))
            {
                return;
            }
            if (lambda.Body.NodeType != ExpressionType.MemberAccess)
            {
                InternalVisit(lambda.Body, sqlBuilder, parameters);
            }
            else
            {
                lambda = System.Linq.Expressions.Expression.Lambda(System.Linq.Expressions.Expression.Equal(lambda.Body, ConstantDefined.BooleanTrue), lambda.Parameters.ToArray());
                InternalVisit(lambda.Body, sqlBuilder, parameters);
            }
        }

        private void VisitBinary(System.Linq.Expressions.Expression ex, StringBuilder sqlBuilder, DynamicParameters parameters)
        {
            Debug.WriteLine(nameof(VisitBinary) + sqlBuilder);
            if (!(ex is BinaryExpression exp))
            {
                return;
            }
            InternalVisit(exp.Left, sqlBuilder, parameters);
            switch (ex.NodeType)
            {
                case ExpressionType.Equal:
                    sqlBuilder.Append(RetValueIsNull(exp.Right) ? " IS NULL " : " = ");
                    break;
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    sqlBuilder.Append(" AND ");
                    break;
                case ExpressionType.GreaterThan:
                    sqlBuilder.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sqlBuilder.Append(" >= ");
                    break;
                case ExpressionType.LessThan:
                    sqlBuilder.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sqlBuilder.Append(" <= ");
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    sqlBuilder.Append(" OR ");
                    break;
                case ExpressionType.Not:
                case ExpressionType.NotEqual:
                    sqlBuilder.Append(RetValueIsNull(exp.Right) ? " IS NOT NULL " : " <> ");
                    break;
                default:
                    throw new NotSupportedException($"暂不支持{ex.NodeType}操作符");
            }
            if (!RetValueIsNull(exp.Right))
            {
                InternalVisit(exp.Right, sqlBuilder, parameters);
            }
        }

        /// <summary>
        /// 判定 exp 返回值肯定是 null
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        private static bool RetValueIsNull(System.Linq.Expressions.Expression exp)
        {
            if (exp.NodeType == ExpressionType.Constant)
            {
                var c = (ConstantExpression)exp;
                return c.Value == null || c.Value == DBNull.Value;
            }
            return false;
        }


        private void VisitMember(System.Linq.Expressions.Expression e, StringBuilder sqlBuilder, DynamicParameters parameters)
        {
            Debug.WriteLine(nameof(VisitMember) + sqlBuilder);
            if (!(e is MemberExpression memberExpression))
            {
                return;
            }
            MemberInfo member = memberExpression.Member;
            if (member.DeclaringType == ConstantDefined.TypeOfDateTime)
            {
                _adapter.HandleDateTime(this, memberExpression, sqlBuilder, parameters);
                return;
            }
            if (memberExpression.Expression == null)
            {
                throw new NotSupportedException();
            }
            if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
            {
                _adapter.AppendColumnName(sqlBuilder, member);
                return;
            }
            if (member.DeclaringType == ConstantDefined.TypeOfString)
            {
                if (_adapter.HandleStringLength(memberExpression, this, sqlBuilder, parameters))
                {
                    return;
                }
            }
            if (member.Name == "Value" && memberExpression.Expression.Type.IsNullable())
            {
                InternalVisit(memberExpression.Expression, sqlBuilder, parameters);
                return;
            }
            System.Linq.Expressions.Expression memberNewExpression = ExpressionEvaluator.MakeExpression(memberExpression);
            InternalVisit(memberNewExpression, sqlBuilder, parameters);
        }


        private void VisitConstant(System.Linq.Expressions.Expression e, StringBuilder sqlBuilder, DynamicParameters parameters)
        {
            Debug.WriteLine(nameof(VisitConstant) + sqlBuilder);
            if (!(e is ConstantExpression constant))
            {
                return;
            }
            if (constant.Value == null)
            {
                sqlBuilder.Append(" IS NULL");
            }
            else
            {
                AddParameter(sqlBuilder, parameters, constant.Value);
            }
        }

        /// <summary>
        /// 一元运算，如i++
        /// </summary>
        private void VisitUnaryNot(System.Linq.Expressions.Expression e, StringBuilder sqlBuilder, DynamicParameters parameters)
        {
            Debug.WriteLine(nameof(VisitUnaryNot) + sqlBuilder);
            if (!(e is UnaryExpression unaryExpression))
            {
                return;
            }
            if (unaryExpression.Operand.NodeType == ExpressionType.MemberAccess)
            {
                BinaryExpression binaryExpression = System.Linq.Expressions.Expression.MakeBinary(ExpressionType.Equal, unaryExpression.Operand, ConstantDefined.BooleanFalse);
                InternalVisit(binaryExpression, sqlBuilder, parameters);
            }
        }

        /// <summary>
        /// 列表初始化表达式
        /// </summary>
        private void VisitUnaryConvert(System.Linq.Expressions.Expression e, StringBuilder sqlBuilder, DynamicParameters parameters)
        {
            Debug.WriteLine(nameof(VisitUnaryConvert) + sqlBuilder);
            if (!(e is UnaryExpression exp))
            {
                return;
            }
            if (exp.Operand.NodeType == ExpressionType.MemberAccess)
            {
                InternalVisit(exp.Operand, sqlBuilder, parameters);
            }
        }

        /// <summary>
        /// 方法调用
        /// </summary>
        private void VisitMethodCall(System.Linq.Expressions.Expression e, StringBuilder sqlBuilder, DynamicParameters parameters)
        {
            Debug.WriteLine(nameof(VisitMethodCall));
            if (!(e is MethodCallExpression methodCall))
            {
                return;
            }
            AbstractMethodCallHandler handler = MethodCallProvider.GetCallHandler(methodCall.Method);
            if (handler != null)
            {
                handler.Handle(methodCall, this, sqlBuilder, parameters);
                return;
            }
            System.Linq.Expressions.Expression callExpression = ExpressionEvaluator.MakeExpression(methodCall);
            InternalVisit(callExpression, sqlBuilder, parameters);
        }

        /// <summary>
        /// 处理日期
        /// </summary>
        internal void DateTimeAddMethod(MethodCallExpression e, string function, StringBuilder sqlBuilder, DynamicParameters parameters)
        {
            _adapter.DateTimeAddMethod(this, e, function, sqlBuilder, parameters);
        }

        /// <summary>
        /// 实例化对象表达式
        /// </summary>
        private void VisitNew(System.Linq.Expressions.Expression e, StringBuilder sqlBuilder, DynamicParameters parameters)
        {
            Debug.WriteLine(nameof(VisitNew) + sqlBuilder);
            if (!(e is NewExpression newExpression))
            {
                return;
            }
            object[] arguments = newExpression.Arguments.Select(ExpressionEvaluator.Visit).ToArray();
            object instance = newExpression.Constructor.Invoke(arguments);
            if (newExpression.Members != null && newExpression.Members.Any())
            {
                foreach (MemberInfo memberInfo in newExpression.Members)
                {
                    if (newExpression.Members.IndexOf(memberInfo) > 0)
                    {
                        sqlBuilder.Append(",");
                    }
                    object value = memberInfo.GetValue(instance);
                    string columnName = _adapter.GetQuoteName(memberInfo);
                    AddParameter(sqlBuilder, parameters, columnName, value);
                }
            }
            else
            {
                AddParameter(sqlBuilder, parameters, instance);
            }
        }

        /// <summary>
        /// 初始化属性表达式
        /// </summary>
        private void VisitMemberInit(System.Linq.Expressions.Expression e, StringBuilder sqlBuilder, DynamicParameters parameters)
        {
            Debug.WriteLine(nameof(VisitMemberInit) + sqlBuilder);
            if (!(e is MemberInitExpression initExpression))
            {
                return;
            }
            foreach (MemberBinding memberBinding in initExpression.Bindings)
            {
                if (memberBinding.BindingType != MemberBindingType.Assignment)
                {
                    throw new NotSupportedException();
                }
                int index = initExpression.Bindings.IndexOf(memberBinding);
                if (index > 0 && index < initExpression.Bindings.Count - 1)
                {
                    sqlBuilder.Append(",");
                }
                MemberAssignment memberAssignment = (MemberAssignment)memberBinding;
                object value = ExpressionEvaluator.Evaluate(memberAssignment.Expression);
                string columnName = _adapter.GetQuoteName(memberBinding.Member);
                AddParameter(sqlBuilder, parameters, columnName, value);
            }
        }

        /// <summary>
        /// 初始化属性表达式
        /// </summary>
        private void VisitNewArray(System.Linq.Expressions.Expression e, StringBuilder sqlBuilder, DynamicParameters parameters)
        {
            Debug.WriteLine(nameof(VisitNewArray) + sqlBuilder);
            if (!(e is NewArrayExpression arrayExpression)) return;
            object value = ExpressionEvaluator.Evaluate(arrayExpression);
            AddParameter(sqlBuilder, parameters, value);
        }

        /// <summary>
        /// 列表初始化表达式
        /// </summary>
        private void VisitListInit(System.Linq.Expressions.Expression e, StringBuilder sqlBuilder, DynamicParameters parameters)
        {
            if (!(e is ListInitExpression initExpression)) return;
            object value = ExpressionEvaluator.Evaluate(initExpression);
            AddParameter(sqlBuilder, parameters, value);
        }

        internal void InternalVisit(System.Linq.Expressions.Expression ex, StringBuilder sqlBuilder, DynamicParameters parameters)
        {
            Debug.WriteLine(nameof(InternalVisit) + sqlBuilder);
            Action<System.Linq.Expressions.Expression, StringBuilder, DynamicParameters> visitor = VisitorProvider.GetVisitor(ex.NodeType);
            visitor.Invoke(ex, sqlBuilder, parameters);
        }

        /// <summary>
        /// 拆分表达式
        /// </summary>
        private void Segregate(System.Linq.Expressions.Expression ex)
        {
            switch (ex.NodeType)
            {
                case ExpressionType.Lambda:
                    Segregate(((LambdaExpression)ex).Body);
                    break;
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    BinaryExpression andEx = (BinaryExpression)ex;
                    SegregatePart(andEx.Left);
                    _nodeTypes.Add(ex.NodeType);
                    SegregatePart(andEx.Right);
                    break;
                default:
                    _nodeExpressions.Add(ex);
                    break;
            }
        }

        private void SegregatePart(System.Linq.Expressions.Expression e)
        {
            if (!ConstantDefined.AndAlsoNodeTypes.Contains(e.NodeType))
            {
                _nodeExpressions.Add(e);
            }
            else
            {
                Segregate(e);
            }
        }

        internal void Visit(System.Linq.Expressions.Expression ex, StringBuilder sqlBuilder, DynamicParameters parameters)
        {
            Debug.WriteLine(nameof(Visit) + sqlBuilder);
            Segregate(ex);
            foreach (System.Linq.Expressions.Expression e in _nodeExpressions)
            {
                int index = _nodeExpressions.IndexOf(e);
                if (index > 0 && index < _nodeExpressions.Count)
                {
                    ExpressionType node = _nodeTypes[index - 1];
                    switch (node)
                    {
                        case ExpressionType.Or:
                        case ExpressionType.OrElse:
                            sqlBuilder.Append(" OR ");
                            break;
                        case ExpressionType.And:
                        case ExpressionType.AndAlso:
                            sqlBuilder.Append(" AND ");
                            break;
                    }
                }
                if (ConstantDefined.OrAlsoNodeTypes.Contains(e.NodeType))
                {
                    sqlBuilder.Append("(");
                    InternalVisit(e, sqlBuilder, parameters);
                    sqlBuilder.Append(")");
                }
                else
                {
                    InternalVisit(e, sqlBuilder, parameters);
                }
            }
            _nodeExpressions.Clear();
            _nodeTypes.Clear();
        }
    }
}
