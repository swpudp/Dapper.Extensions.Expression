using Dapper.Extensions.Expression.Adapters;
using Dapper.Extensions.Expression.Extensions;
using Dapper.Extensions.Expression.Utilities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Dapper.Extensions.Expression.Visitors
{
    /// <summary>
    /// update表达式解析
    /// </summary>
    internal static class UpdateExpressionVisitor
    {
        public static void Visit(System.Linq.Expressions.Expression e, ISqlAdapter adapter, StringBuilder builder, DynamicParameters parameters)
        {
            switch (e.NodeType)
            {
                case ExpressionType.Lambda:
                    VisitLambda((LambdaExpression)e, adapter, builder, parameters);
                    break;
                case ExpressionType.New:
                    VisitNew((NewExpression)e, adapter, builder, parameters);
                    break;
                case ExpressionType.MemberInit:
                    VisitMemberInit((MemberInitExpression)e, adapter, builder, parameters);
                    break;
                case ExpressionType.MemberAccess:
                    VisitMemberAccess((MemberExpression)e, adapter, builder, parameters);
                    break;
                case ExpressionType.Constant:
                    VisitConstant((ConstantExpression)e, builder);
                    break;
                case ExpressionType.Convert:
                    VisitUnaryConvert((UnaryExpression)e, adapter, builder, parameters);
                    break;
                case ExpressionType.Add:
                    VisitBinaryAdd((BinaryExpression)e, adapter, builder, parameters);
                    break;
                case ExpressionType.Subtract:
                    VisitBinarySubtract((BinaryExpression)e, adapter, builder, parameters);
                    break;
                case ExpressionType.Multiply:
                    VisitBinaryMultiply((BinaryExpression)e, adapter, builder, parameters);
                    break;
                case ExpressionType.Divide:
                    VisitBinaryDivide((BinaryExpression)e, adapter, builder, parameters);
                    break;
                case ExpressionType.Not:
                    VisitUnaryNot((UnaryExpression)e, adapter, builder, parameters);
                    break;
                case ExpressionType.NotEqual:
                    VisitBinaryNotEqual((BinaryExpression)e, builder);
                    break;
                case ExpressionType.Equal:
                    VisitBinaryEqual((BinaryExpression)e, builder);
                    break;
                case ExpressionType.Call:
                    VisitCall((MethodCallExpression)e, builder, parameters);
                    break;
                default:
                    throw new NotSupportedException($"不支持{e.NodeType}");
            }
        }


        private static void VisitCall(MethodCallExpression call, StringBuilder builder, DynamicParameters parameters)
        {
            object value = ExpressionEvaluator.Visit(call);
            AddParameter(builder, parameters, value);
        }

        private static void VisitBinaryAdd(BinaryExpression binary, ISqlAdapter adapter, StringBuilder builder, DynamicParameters parameters)
        {
            Visit(binary.Left, adapter, builder, parameters);
            builder.Append(" + ");
            Visit(binary.Right, adapter, builder, parameters);
        }

        private static void VisitBinarySubtract(BinaryExpression binary, ISqlAdapter adapter, StringBuilder builder, DynamicParameters parameters)
        {
            Visit(binary.Left, adapter, builder, parameters);
            builder.Append(" - ");
            Visit(binary.Right, adapter, builder, parameters);
        }

        private static void VisitBinaryMultiply(BinaryExpression binary, ISqlAdapter adapter, StringBuilder builder, DynamicParameters parameters)
        {
            Visit(binary.Left, adapter, builder, parameters);
            builder.Append(" * ");
            Visit(binary.Right, adapter, builder, parameters);
        }

        private static void VisitBinaryDivide(BinaryExpression binary, ISqlAdapter adapter, StringBuilder builder, DynamicParameters parameters)
        {
            Visit(binary.Left, adapter, builder, parameters);
            builder.Append(" / ");
            Visit(binary.Right, adapter, builder, parameters);
        }

        private static void VisitBinaryNotEqual(BinaryExpression binary, StringBuilder builder)
        {
            object left = ExpressionEvaluator.Visit(binary.Left);
            object right = ExpressionEvaluator.Visit(binary.Right);
            bool result = !Equals(left, right);
            builder.AppendFormat("{0}", result ? 1 : 0);
        }

        private static void VisitBinaryEqual(BinaryExpression binary, StringBuilder builder)
        {
            object left = ExpressionEvaluator.Visit(binary.Left);
            object right = ExpressionEvaluator.Visit(binary.Right);
            bool result = Equals(left, right);
            builder.AppendFormat("{0}", result ? 1 : 0);
        }

        private static void VisitLambda(LambdaExpression lambda, ISqlAdapter adapter, StringBuilder builder, DynamicParameters parameters)
        {
            var newExp = new ReplaceExpressionVisitor(lambda.Parameters, false).Visit(lambda) as LambdaExpression;
            Visit(newExp.Body, adapter, builder, parameters);
        }

        private static void VisitNew(NewExpression e, ISqlAdapter adapter, StringBuilder builder, DynamicParameters parameters)
        {
            if (e.Members == null || !e.Members.Any())
            {
                throw new InvalidOperationException("No Members found in type:" + e.Type.FullName);
            }
            //object[] arguments = e.Arguments.Select(ExpressionEvaluator.Visit).ToArray();
            //object instance = e.Constructor.Invoke(arguments);
            foreach (MemberInfo memberInfo in e.Members)
            {
                int index = e.Members.IndexOf(memberInfo);
                if (index > 0)
                {
                    builder.Append(',');
                }
                string columnName = adapter.GetQuoteName(memberInfo, out _);
                builder.Append(columnName).Append('=');

                System.Linq.Expressions.Expression argExpression = e.Arguments[index];
                if (!CanEvaluate(argExpression, adapter, builder, parameters))
                {
                    continue;
                }
                object value = ExpressionEvaluator.Visit(argExpression);
                AddParameter(builder, parameters, value);
            }
        }

        private static void VisitMemberInit(MemberInitExpression e, ISqlAdapter adapter, StringBuilder builder, DynamicParameters parameters)
        {
            foreach (MemberBinding memberBinding in e.Bindings)
            {
                if (memberBinding.BindingType != MemberBindingType.Assignment)
                {
                    throw new NotSupportedException();
                }
                if (memberBinding.Member.IsNotMapped())
                {
                    throw new NotSupportedException($"NotMappedAttribute marked on property:{memberBinding.Member.Name} of type:{memberBinding.Member.DeclaringType?.FullName}");
                }
                int index = e.Bindings.IndexOf(memberBinding);
                if (index > 0 && index < e.Bindings.Count)
                {
                    builder.Append(',');
                }
                string columnName = adapter.GetQuoteName(memberBinding.Member, out _);
                builder.Append(columnName).Append('=');
                MemberAssignment memberAssignment = (MemberAssignment)memberBinding;
                if (!CanEvaluate(memberAssignment.Expression, adapter, builder, parameters))
                {
                    continue;
                }
                object value = ExpressionEvaluator.Visit(memberAssignment.Expression);
                AddParameter(builder, parameters, value);
            }
        }

        private static bool CanEvaluate(System.Linq.Expressions.Expression e, ISqlAdapter adapter, StringBuilder builder, DynamicParameters parameters)
        {
            switch (e.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                    Visit((BinaryExpression)e, adapter, builder, parameters);
                    return false;
                case ExpressionType.Convert:
                case ExpressionType.Not:
                    Visit((UnaryExpression)e, adapter, builder, parameters);
                    return false;
                case ExpressionType.MemberAccess:
                    MemberExpression mExp = (MemberExpression)e;
                    if (mExp.Expression?.NodeType != ExpressionType.Parameter)
                    {
                        return true;
                    }
                    Visit(mExp, adapter, builder, parameters);
                    return false;
                default:
                    return true;
            }
        }

        private static void AddParameter(StringBuilder builder, DynamicParameters parameters, object value)
        {
            int index = parameters.ParameterNames.Count();
            string parameterName = $"@u_p_{index + 1}";
            builder.Append(parameterName);
            parameters.Add(parameterName, value);
        }

        private static void VisitMemberAccess(MemberExpression m, ISqlAdapter adapter, StringBuilder builder, DynamicParameters parameters)
        {
            if (m.Type == ConstantDefined.TypeOfDateTime)
            {
                object value = ExpressionEvaluator.Visit(m);
                AddParameter(builder, parameters, value);
                return;
            }
            if (m.Expression?.NodeType == ExpressionType.Parameter)
            {
                builder.AppendFormat("{0}", adapter.GetQuoteName(m.Member, out _));
            }
            else
            {
                object value = ExpressionEvaluator.Visit(m);
                AddParameter(builder, parameters, value);
            }
        }

        private static void VisitConstant(ConstantExpression e, StringBuilder builder)
        {
            builder.Append(e.Value);
        }

        private static void VisitUnaryConvert(UnaryExpression u, ISqlAdapter adapter, StringBuilder builder, DynamicParameters parameters)
        {
            Visit(u.Operand, adapter, builder, parameters);
        }

        private static void VisitUnaryNot(UnaryExpression u, ISqlAdapter adapter, StringBuilder builder, DynamicParameters parameters)
        {
            builder.Append("CASE ");
            Visit(u.Operand, adapter, builder, parameters);
            builder.Append(" WHEN 1 THEN 0 ELSE 1 END");
        }
    }
}
