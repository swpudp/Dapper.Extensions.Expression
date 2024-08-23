using Dapper.Extensions.Expression.Adapters;
using System;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extensions.Expression.Visitors
{
    internal static class OnExpressionVisitor
    {
        public static void Visit(System.Linq.Expressions.Expression e, ISqlAdapter adapter, StringBuilder builder)
        {
            switch (e.NodeType)
            {
                case ExpressionType.Lambda:
                    VisitLambda((LambdaExpression)e, adapter, builder);
                    break;
                case ExpressionType.MemberAccess:
                    VisitMemberAccess((MemberExpression)e, adapter, builder);
                    break;
                case ExpressionType.OrElse:
                    VisitBinary((BinaryExpression)e, ExpressionType.OrElse, adapter, builder);
                    break;
                case ExpressionType.AndAlso:
                    VisitBinary((BinaryExpression)e, ExpressionType.AndAlso, adapter, builder);
                    break;
                case ExpressionType.Equal:
                    VisitBinary((BinaryExpression)e, ExpressionType.Equal, adapter, builder);
                    break;
                case ExpressionType.Parameter:
                    ParameterExpression parameterExpression = (ParameterExpression)e;
                    builder.Append(adapter.GetQuoteName(parameterExpression.Name)).Append('.');
                    break;
                default:
                    throw new NotSupportedException($"不支持{e.NodeType}");
            }
        }

        private static void VisitLambda(LambdaExpression lambda, ISqlAdapter adapter, StringBuilder builder)
        {
            LambdaExpression newExp = ReplaceParameterVisitor.Replace(lambda, lambda.Parameters);
            Visit(newExp.Body, adapter, builder);
        }

        private static void VisitBinary(BinaryExpression binary, ExpressionType nodeType, ISqlAdapter adapter, StringBuilder builder)
        {
            Visit(binary.Left, adapter, builder);
            switch (nodeType)
            {
                case ExpressionType.Equal:
                    builder.Append(" = ");
                    break;
                case ExpressionType.AndAlso:
                    builder.Append(" AND ");
                    break;
                case ExpressionType.OrElse:
                    builder.Append(" OR ");
                    break;
                default:
                    throw new NotSupportedException($"不支持{nodeType}");
            }
            Visit(binary.Right, adapter, builder);
        }

        private static void VisitMemberAccess(MemberExpression m, ISqlAdapter adapter, StringBuilder builder)
        {
            Visit(m.Expression, adapter, builder);
            adapter.AppendColumnName(builder, m.Member);
        }
    }
}
