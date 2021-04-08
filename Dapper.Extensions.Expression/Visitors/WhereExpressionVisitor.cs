using Dapper.Extensions.Expression.Adapters;
using Dapper.Extensions.Expression.Extensions;
using Dapper.Extensions.Expression.MethodCalls;
using Dapper.Extensions.Expression.Providers;
using Dapper.Extensions.Expression.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Dapper.Extensions.Expression.Visitors
{
    internal static class WhereExpressionVisitor
    {
        /// <summary>
        /// 表达式访问方法
        /// </summary>
        private static readonly IDictionary<ExpressionType, Action<System.Linq.Expressions.Expression, ISqlAdapter, StringBuilder, DynamicParameters, bool>> Visitors = new Dictionary<ExpressionType, Action<System.Linq.Expressions.Expression, ISqlAdapter, StringBuilder, DynamicParameters, bool>>
        {
            [ExpressionType.Lambda] = VisitLambda,
            [ExpressionType.Equal] = VisitBinary,
            [ExpressionType.Add] = VisitBinary,
            [ExpressionType.AndAlso] = VisitBinary,
            [ExpressionType.GreaterThan] = VisitBinary,
            [ExpressionType.GreaterThanOrEqual] = VisitBinary,
            [ExpressionType.LessThan] = VisitBinary,
            [ExpressionType.LessThanOrEqual] = VisitBinary,
            [ExpressionType.Or] = VisitBinary,
            [ExpressionType.OrElse] = VisitBinary,
            [ExpressionType.NotEqual] = VisitBinary,
            [ExpressionType.MemberAccess] = VisitMember,
            [ExpressionType.Constant] = VisitConstant,
            [ExpressionType.Call] = VisitMethodCall,
            [ExpressionType.Not] = VisitUnaryNot,
            [ExpressionType.Convert] = VisitUnaryConvert,
            [ExpressionType.New] = VisitNew,
            [ExpressionType.MemberInit] = VisitMemberInit,
            [ExpressionType.NewArrayInit] = VisitNewArray,
            [ExpressionType.ListInit] = VisitListInit,
            [ExpressionType.Parameter] = VisitParameter
        };

        internal static void AddParameter(StringBuilder sqlBuilder, DynamicParameters parameters, object value)
        {
            int paramIndex = parameters.ParameterNames.Count();
            string parameterName = $"@w_p_{paramIndex + 1}";
            sqlBuilder.Append(parameterName);
            parameters.Add(parameterName, value);
        }

        private static void AddParameter(StringBuilder sqlBuilder, DynamicParameters parameters, string columnName, object value)
        {
            int paramIndex = parameters.ParameterNames.Count();
            string parameterName = $"@w_p_{paramIndex + 1}";
            sqlBuilder.AppendFormat("{0}={1}", columnName, parameterName);
            parameters.Add(parameterName, value);
        }

        private static void VisitLambda(System.Linq.Expressions.Expression e, ISqlAdapter adapter, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            Debug.WriteLine(nameof(VisitLambda) + sqlBuilder);

            if (!(e is LambdaExpression lambda))
            {
                return;
            }
            if (lambda.Body.NodeType != ExpressionType.MemberAccess)
            {
                InternalVisit(lambda.Body, adapter, sqlBuilder, parameters, appendParameter);
            }
            else
            {
                lambda = System.Linq.Expressions.Expression.Lambda(System.Linq.Expressions.Expression.Equal(lambda.Body, ConstantDefined.BooleanTrue), lambda.Parameters.ToArray());
                InternalVisit(lambda.Body, adapter, sqlBuilder, parameters, appendParameter);
            }
        }

        private static void VisitBinary(System.Linq.Expressions.Expression ex, ISqlAdapter adapter, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            Debug.WriteLine(nameof(VisitBinary) + sqlBuilder);
            if (!(ex is BinaryExpression exp))
            {
                return;
            }
            InternalVisit(exp.Left, adapter, sqlBuilder, parameters, appendParameter);
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
                case ExpressionType.Add:
                    sqlBuilder.Append(" + ");
                    break;
                default:
                    throw new NotSupportedException($"暂不支持{ex.NodeType}操作符");
            }
            if (!RetValueIsNull(exp.Right))
            {
                InternalVisit(exp.Right, adapter, sqlBuilder, parameters, appendParameter);
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
                ConstantExpression c = (ConstantExpression)exp;
                return c.Value == null || c.Value == DBNull.Value;
            }
            if (exp.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression m = (MemberExpression)exp;
                if (m.Expression?.NodeType == ExpressionType.Parameter)
                {
                    return false;
                }
                object value = ExpressionEvaluator.Visit(exp);
                return value == null;
            }
            return false;
        }

        private static void VisitMember(System.Linq.Expressions.Expression e, ISqlAdapter adapter, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            Debug.WriteLine(nameof(VisitMember) + sqlBuilder);
            if (!(e is MemberExpression memberExpression))
            {
                return;
            }
            MemberInfo member = memberExpression.Member;
            if (member.IsNotMapped())
            {
                throw new NotSupportedException($"NotMappedAttribute marked on property:{ member.Name} of type:{member.DeclaringType?.FullName}");
            }
            if (member.DeclaringType == ConstantDefined.TypeOfDateTime)
            {
                adapter.HandleDateTime(memberExpression, sqlBuilder, parameters, appendParameter);
                return;
            }
            if (memberExpression.Expression == null)
            {
                throw new NotSupportedException();
            }
            if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
            {
                //todo 访问参数，无法从父类获取子类特性
                InternalVisit(memberExpression.Expression, adapter, sqlBuilder, parameters, appendParameter);
                MemberInfo columnProperty = TypeProvider.GetColumnProperty(memberExpression.Expression.Type, member);
                adapter.AppendColumnName(sqlBuilder, columnProperty);
                return;
            }
            if (member.DeclaringType == ConstantDefined.TypeOfString)
            {
                if (adapter.HandleStringLength(memberExpression, sqlBuilder, parameters, appendParameter))
                {
                    return;
                }
            }
            if (memberExpression.Expression.Type.IsNullable())
            {
                if (member.Name == ConstantDefined.MemberNameValue)
                {
                    InternalVisit(memberExpression.Expression, adapter, sqlBuilder, parameters, appendParameter);
                    return;
                }
                if (member.Name == ConstantDefined.MemberNameHasValue)
                {
                    adapter.AppendColumnName(sqlBuilder, memberExpression.Member);
                    return;
                }
            }
            System.Linq.Expressions.Expression memberNewExpression = ExpressionEvaluator.MakeExpression(memberExpression);
            InternalVisit(memberNewExpression, adapter, sqlBuilder, parameters, appendParameter);
        }


        private static void VisitConstant(System.Linq.Expressions.Expression e, ISqlAdapter adapter, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
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
        private static void VisitUnaryNot(System.Linq.Expressions.Expression e, ISqlAdapter adapter, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            Debug.WriteLine(nameof(VisitUnaryNot) + sqlBuilder);
            if (!(e is UnaryExpression unaryExpression))
            {
                return;
            }
            if (unaryExpression.Operand.NodeType == ExpressionType.MemberAccess)
            {
                BinaryExpression binaryExpression = System.Linq.Expressions.Expression.MakeBinary(ExpressionType.Equal, unaryExpression.Operand, ConstantDefined.BooleanFalse);
                InternalVisit(binaryExpression, adapter, sqlBuilder, parameters, appendParameter);
            }
        }

        /// <summary>
        /// 列表初始化表达式
        /// </summary>
        private static void VisitUnaryConvert(System.Linq.Expressions.Expression e, ISqlAdapter adapter, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            Debug.WriteLine(nameof(VisitUnaryConvert) + sqlBuilder);
            if (!(e is UnaryExpression exp))
            {
                return;
            }
            InternalVisit(exp.Operand, adapter, sqlBuilder, parameters, appendParameter);
        }

        /// <summary>
        /// 方法调用
        /// </summary>
        private static void VisitMethodCall(System.Linq.Expressions.Expression e, ISqlAdapter adapter, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            Debug.WriteLine(nameof(VisitMethodCall));
            if (!(e is MethodCallExpression exp))
            {
                return;
            }
            AbstractMethodCallHandler handler = MethodCallProvider.GetCallHandler(exp);
            if (handler != null)
            {
                handler.Handle(exp, adapter, sqlBuilder, parameters, appendParameter);
                return;
            }
            System.Linq.Expressions.Expression callExpression = ExpressionEvaluator.MakeExpression(exp);
            InternalVisit(callExpression, adapter, sqlBuilder, parameters, appendParameter);
        }

        /// <summary>
        /// 实例化对象表达式
        /// </summary>
        private static void VisitNew(System.Linq.Expressions.Expression e, ISqlAdapter adapter, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
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
                    string columnName = adapter.GetQuoteName(memberInfo, out _);
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
        private static void VisitMemberInit(System.Linq.Expressions.Expression e, ISqlAdapter adapter, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
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
                if (index > 0 && index < initExpression.Bindings.Count)
                {
                    sqlBuilder.Append(",");
                }
                MemberAssignment memberAssignment = (MemberAssignment)memberBinding;
                object value = ExpressionEvaluator.Visit(memberAssignment.Expression);
                string columnName = adapter.GetQuoteName(memberBinding.Member, out _);
                AddParameter(sqlBuilder, parameters, columnName, value);
            }
        }

        /// <summary>
        /// 初始化属性表达式
        /// </summary>
        private static void VisitNewArray(System.Linq.Expressions.Expression e, ISqlAdapter adapter, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            Debug.WriteLine(nameof(VisitNewArray) + sqlBuilder);
            if (!(e is NewArrayExpression arrayExpression)) return;
            object value = ExpressionEvaluator.Visit(arrayExpression);
            AddParameter(sqlBuilder, parameters, value);
        }

        /// <summary>
        /// 列表初始化表达式
        /// </summary>
        private static void VisitListInit(System.Linq.Expressions.Expression e, ISqlAdapter adapter, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            if (!(e is ListInitExpression initExpression)) return;
            object value = ExpressionEvaluator.Visit(initExpression);
            AddParameter(sqlBuilder, parameters, value);
        }

        /// <summary>
        /// 列表参数表达式
        /// </summary>
        private static void VisitParameter(System.Linq.Expressions.Expression e, ISqlAdapter adapter, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            if (!appendParameter) return;
            if (!(e is ParameterExpression parameterExpression)) return;
            string quoteName = adapter.GetQuoteName(parameterExpression.Name);
            sqlBuilder.Append(quoteName).Append(".");
        }

        /// <summary>
        /// 访问表达式
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="adapter"></param>
        /// <param name="sqlBuilder"></param>
        /// <param name="parameters"></param>
        /// <param name="appendParameter"></param>
        internal static void InternalVisit(System.Linq.Expressions.Expression ex, ISqlAdapter adapter, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            Debug.WriteLine(nameof(InternalVisit) + sqlBuilder);
            if (!Visitors.TryGetValue(ex.NodeType, out Action<System.Linq.Expressions.Expression, ISqlAdapter, StringBuilder, DynamicParameters, bool> visitor))
            {
                throw new NotSupportedException(ex.NodeType.ToString());
            }
            visitor(ex, adapter, sqlBuilder, parameters, appendParameter);
        }

        /// <summary>
        /// 拆分表达式
        /// 将xx && (xxx || xx)、(xxx || xxx) && (xxx || xxx)等这类表达式拆分开，使翻译SQL形式是：xxx AND (xxx OR xxx)、(xxx OR xxx) AND (xxx OR xxx)
        /// </summary>
        private static void Segregate(BinaryExpression ex, ExpressionType nodeType, IList<System.Linq.Expressions.Expression> nodeExpressions)
        {
            if (nodeType == ex.Left.NodeType)
            {
                Segregate((BinaryExpression)ex.Left, nodeType, nodeExpressions);
            }
            else
            {
                nodeExpressions.Add(ex.Left);
            }
            if (nodeType == ex.Right.NodeType)
            {
                Segregate((BinaryExpression)ex.Right, nodeType, nodeExpressions);
            }
            else
            {
                nodeExpressions.Add(ex.Right);
            }
        }

        /// <summary>
        /// SQL和二元运算连接对应关系
        /// </summary>
        private static readonly IDictionary<ExpressionType, string> BinaryTypes = new Dictionary<ExpressionType, string>
        {
            [ExpressionType.AndAlso] = "AND",
            [ExpressionType.OrElse] = "OR"
        };

        internal static void Visit(System.Linq.Expressions.Expression ex, ISqlAdapter adapter, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            if (!(ex is LambdaExpression lambda))
            {
                throw new NotSupportedException();
            }
            if (lambda.Body.NodeType != ExpressionType.AndAlso && lambda.Body.NodeType != ExpressionType.OrElse)
            {
                InternalVisit(ex, adapter, sqlBuilder, parameters, appendParameter);
                return;
            }
            sqlBuilder.Append("(");
            BinaryExpression b = (BinaryExpression)lambda.Body;
            IList<System.Linq.Expressions.Expression> nodeExpressions = new List<System.Linq.Expressions.Expression>();
            Segregate(b, b.NodeType, nodeExpressions);
            foreach (System.Linq.Expressions.Expression e in nodeExpressions)
            {
                if (nodeExpressions.IndexOf(e) > 0)
                {
                    sqlBuilder.AppendFormat(" {0} ", BinaryTypes[b.NodeType]);
                }
                if (e.NodeType == ExpressionType.AndAlso || e.NodeType == ExpressionType.OrElse)
                {
                    IList<System.Linq.Expressions.Expression> children = new List<System.Linq.Expressions.Expression>();
                    Segregate((BinaryExpression)e, e.NodeType, children);
                    foreach (System.Linq.Expressions.Expression child in children)
                    {
                        int childIndex = children.IndexOf(child);
                        if (childIndex == 0)
                        {
                            sqlBuilder.Append("(");
                        }
                        bool isChildLast = childIndex >= children.Count - 1;
                        Visit(child, sqlBuilder, adapter, parameters, appendParameter);
                        if (isChildLast)
                        {
                            sqlBuilder.Append(")");
                        }
                        else
                        {
                            sqlBuilder.AppendFormat(" {0} ", BinaryTypes[e.NodeType]);
                        }
                    }
                }
                else
                {
                    Visit(e, sqlBuilder, adapter, parameters, appendParameter);
                }
            }
            sqlBuilder.Append(")");
            nodeExpressions.Clear();
        }


        private static void Visit(System.Linq.Expressions.Expression e, StringBuilder sqlBuilder, ISqlAdapter adapter, DynamicParameters parameters, bool appendParameter)
        {
            if (e.NodeType == ExpressionType.OrElse || e.NodeType == ExpressionType.AndAlso)
            {
                sqlBuilder.Append("(");
                InternalVisit(e, adapter, sqlBuilder, parameters, appendParameter);
                sqlBuilder.Append(")");
            }
            else
            {
                InternalVisit(e, adapter, sqlBuilder, parameters, appendParameter);
            }
        }
    }
}
