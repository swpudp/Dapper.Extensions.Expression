using System;
using System.Linq.Expressions;
using System.Text;
using Dapper.Extensions.Expression.Adapters;
using Dapper.Extensions.Expression.Extensions;

namespace Dapper.Extensions.Expression.Visitors
{
    /// <summary>
    /// select表达式解析
    /// </summary>
    internal static class OrderExpressionVisitor
    {
        public static void Visit(System.Linq.Expressions.Expression e, ISqlAdapter adapter, string order, StringBuilder builder, bool appendParameter)
        {
            switch (e.NodeType)
            {
                case ExpressionType.Lambda:
                    VisitLambda((LambdaExpression)e, adapter, order, builder, appendParameter);
                    break;
                case ExpressionType.New:
                    VisitNew((NewExpression)e, adapter, order, builder, appendParameter);
                    break;
                case ExpressionType.MemberInit:
                    VisitMemberInit((MemberInitExpression)e, adapter, order, builder, appendParameter);
                    break;
                case ExpressionType.Convert:
                    VisitUnary((UnaryExpression)e, adapter, order, builder, appendParameter);
                    break;
                case ExpressionType.MemberAccess:
                    VisitMemberAccess((MemberExpression)e, adapter, order, builder, appendParameter);
                    break;
                case ExpressionType.Parameter:
                    VisitParameter((ParameterExpression)e, adapter, builder);
                    break;
                default:
                    throw new NotSupportedException($"不支持{e.NodeType}");
            }
        }

        private static void VisitLambda(LambdaExpression lambda, ISqlAdapter adapter, string order, StringBuilder builder, bool appendParameter)
        {
            var newExp = ReplaceParameterVisitor.Replace(lambda, lambda.Parameters);
            Visit(newExp.Body, adapter, order, builder, appendParameter);
        }

        private static void VisitNew(NewExpression nex, ISqlAdapter adapter, string order, StringBuilder builder, bool appendParameter)
        {
            foreach (System.Linq.Expressions.Expression e in nex.Arguments)
            {
                if (!(e is MemberExpression member))
                {
                    throw new NotSupportedException("Not MemberExpression");
                }
                if (member.Member.IsNotMapped())
                {
                    throw new NotSupportedException($"NotMappedAttribute marked on property:{member.Member.Name} of type:{member.Member.DeclaringType?.FullName}");
                }
                int index = nex.Arguments.IndexOf(e);
                if (index > 0 && index < nex.Arguments.Count)
                {
                    builder.Append(',');
                }
                if (appendParameter)
                {
                    Visit(member.Expression, adapter, order, builder, true);
                }
                adapter.AppendColumnName(builder, member.Member);
                if (!string.IsNullOrWhiteSpace(order))
                {
                    builder.AppendFormat(" {0} ", order);
                }
            }
        }

        private static void VisitMemberInit(MemberInitExpression init, ISqlAdapter adapter, string order, StringBuilder builder, bool appendParameter)
        {
            foreach (MemberBinding binding in init.Bindings)
            {
                if (!(binding is MemberAssignment assignment))
                {
                    throw new NotSupportedException();
                }
                System.Linq.Expressions.Expression assignmentExp = assignment.Expression;
                if (assignmentExp is UnaryExpression unary)
                {
                    assignmentExp = unary.Operand;
                }
                if (!(assignmentExp is MemberExpression member))
                {
                    throw new NotSupportedException();
                }
                if (member.Member.IsNotMapped())
                {
                    throw new NotSupportedException($"NotMappedAttribute marked on property:{member.Member.Name} of type:{member.Member.DeclaringType?.FullName}");
                }
                int index = init.Bindings.IndexOf(binding);
                if (index > 0 && index < init.Bindings.Count)
                {
                    builder.Append(',');
                }
                if (appendParameter)
                {
                    Visit(member.Expression, adapter, order, builder, true);
                }
                adapter.AppendColumnName(builder, member.Member);
                if (!string.IsNullOrWhiteSpace(order))
                {
                    builder.AppendFormat(" {0} ", order);
                }
            }
        }

        private static void VisitUnary(UnaryExpression u, ISqlAdapter adapter, string order, StringBuilder builder, bool appendParameter)
        {
            Visit(u.Operand, adapter, order, builder, appendParameter);
        }

        private static void VisitMemberAccess(MemberExpression m, ISqlAdapter adapter, string order, StringBuilder builder, bool appendParameter)
        {
            if (appendParameter)
            {
                Visit(m.Expression, adapter, order, builder, true);
            }
            adapter.AppendColumnName(builder, m.Member);
            if (!string.IsNullOrWhiteSpace(order))
            {
                builder.AppendFormat(" {0} ", order);
            }
        }

        private static void VisitParameter(ParameterExpression m, ISqlAdapter adapter, StringBuilder builder)
        {
            builder.Append(adapter.GetQuoteName(m.Name)).Append('.');
        }
    }
}
