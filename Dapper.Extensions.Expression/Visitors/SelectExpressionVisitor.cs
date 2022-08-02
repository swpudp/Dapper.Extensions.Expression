using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Dapper.Extensions.Expression.Adapters;
using Dapper.Extensions.Expression.Extensions;
using Dapper.Extensions.Expression.MethodCalls;
using Dapper.Extensions.Expression.Providers;

namespace Dapper.Extensions.Expression.Visitors
{
    /// <summary>
    /// select表达式解析
    /// </summary>
    internal static class SelectExpressionVisitor
    {
        public static void Visit(System.Linq.Expressions.Expression e, ISqlAdapter adapter, StringBuilder builder, bool appendParameter)
        {
            switch (e.NodeType)
            {
                case ExpressionType.Lambda:
                    VisitLambda((LambdaExpression)e, adapter, builder, appendParameter);
                    break;
                case ExpressionType.New:
                    VisitNew((NewExpression)e, adapter, builder, appendParameter);
                    break;
                case ExpressionType.MemberInit:
                    VisitMemberInit((MemberInitExpression)e, adapter, builder, appendParameter);
                    break;
                case ExpressionType.Convert:
                    VisitUnary((UnaryExpression)e, adapter, builder, appendParameter);
                    break;
                case ExpressionType.MemberAccess:
                    VisitMemberAccess((MemberExpression)e, adapter, builder, appendParameter);
                    break;
                case ExpressionType.Parameter:
                    VisitParameter((ParameterExpression)e, adapter, builder);
                    break;
                case ExpressionType.Call:
                    VisitCall((MethodCallExpression)e, adapter, builder, appendParameter);
                    break;
                case ExpressionType.Coalesce:
                    VisitCoalesce((BinaryExpression)e, adapter, builder, appendParameter);
                    break;
                case ExpressionType.Constant:
                    VisitConstant((ConstantExpression)e, builder);
                    break;
                default:
                    throw new NotSupportedException($"不支持{e.NodeType}");
            }
        }

        private static void VisitLambda(LambdaExpression lambda, ISqlAdapter adapter, StringBuilder builder, bool appendParameter)
        {
            var newExp = new ReplaceExpressionVisitor(lambda.Parameters, true).Visit(lambda) as LambdaExpression;
            Visit(newExp.Body, adapter, builder, appendParameter);
        }

        private static void VisitNew(NewExpression nex, ISqlAdapter adapter, StringBuilder builder, bool appendParameter)
        {
            foreach (MemberInfo member in nex.Members)
            {
                int index = nex.Members.IndexOf(member);
                System.Linq.Expressions.Expression argExp = nex.Arguments[index];
                if (argExp is ConstantExpression constant)
                {
                    if (index > 0 && index < nex.Members.Count)
                    {
                        builder.Append(',');
                    }
                    Visit(constant, builder);
                    builder.Append(" AS ").Append(adapter.GetQuoteName(member.Name));
                    continue;
                }
                if (!(argExp is MemberExpression memberExp))
                {
                    if (index > 0 && index < nex.Members.Count)
                    {
                        builder.Append(',');
                    }
                    Visit(argExp, adapter, builder, appendParameter);
                    adapter.AppendColumnName(builder, member);
                    continue;
                }
                MemberInfo expMember = memberExp.Member;
                if (expMember.IsNotMapped())
                {
                    continue;
                }
                if (index > 0 && index < nex.Members.Count)
                {
                    builder.Append(',');
                }
                if (appendParameter)
                {
                    Visit(memberExp.Expression, adapter, builder, true);
                }
                if (member.Name == expMember.Name)
                {
                    bool isAlias = adapter.AppendColumnName(builder, expMember);
                    if (!isAlias) continue;
                    builder.Append(" AS ");
                    adapter.AppendQuoteName(builder, member.Name);
                }
                else
                {
                    adapter.AppendAliasColumnName(builder, expMember, member);
                }
            }
        }

        private static void VisitConstant(ConstantExpression e, StringBuilder builder)
        {
            builder.Append(e.Value);
        }

        private static void Visit(ConstantExpression exp, StringBuilder sqlBuilder)
        {
            if (exp.Value == null || exp.Value == DBNull.Value)
            {
                sqlBuilder.Append("NULL");
                return;
            }
            var objType = exp.Value.GetType();
            if (objType == ConstantDefined.TypeOfBoolean)
            {
                sqlBuilder.Append((bool)exp.Value ? "1" : "0");
                return;
            }
            if (objType == ConstantDefined.TypeOfString)
            {
                sqlBuilder.AppendFormat("N'{0}'", exp.Value);
                return;
            }
            if (objType.IsEnum)
            {
                sqlBuilder.Append(Convert.ChangeType(exp.Value, Enum.GetUnderlyingType(objType)));
                return;
            }
            if (exp.Value.GetType().IsValueType)
            {
                sqlBuilder.Append(exp.Value);
                return;
            }
            throw new NotSupportedException();
        }

        private static void VisitCoalesce(BinaryExpression e, ISqlAdapter adapter, StringBuilder builder, bool appendParameter)
        {
            builder.Append("CAST(IFNULL(");
            Visit(e.Left, adapter, builder, appendParameter);
            builder.Append(',');
            Visit(e.Right, adapter, builder, appendParameter);
            builder.Append(") AS CHAR) AS ");
        }

        private static void VisitCall(MethodCallExpression e, ISqlAdapter adapter, StringBuilder builder, bool appendParameter)
        {
            if (e.Method.DeclaringType == typeof(Function))
            {
                AbstractMethodCallHandler handler = MethodCallProvider.GetCallHandler(e);
                handler.Handle(e, adapter, builder, null, appendParameter);
                return;
            }
            if (e.Object == null)
            {
                return;
            }
            if (e.Object.Type == ConstantDefined.TypeOfString)
            {
                Visit(e.Object, adapter, builder, appendParameter);
                return;
            }
            Visit(e.Object, adapter, builder, appendParameter);
        }

        private static void VisitMemberInit(MemberInitExpression init, ISqlAdapter adapter, StringBuilder builder, bool appendParameter)
        {
            foreach (MemberBinding binding in init.Bindings)
            {
                if (!(binding is MemberAssignment assignment))
                {
                    throw new NotSupportedException();
                }
                int index = init.Bindings.IndexOf(binding);
                System.Linq.Expressions.Expression assignExp = assignment.Expression;
                if (assignExp is UnaryExpression unary)
                {
                    assignExp = unary.Operand;
                }
                if (assignExp is ConstantExpression constant)
                {
                    if (index > 0 && index < init.Bindings.Count)
                    {
                        builder.Append(',');
                    }
                    Visit(constant, builder);
                    builder.Append(" AS ");
                    adapter.AppendQuoteName(builder, binding.Member.Name);
                    continue;
                }
                if (!(assignExp is MemberExpression member))
                {
                    if (index > 0 && index < init.Bindings.Count)
                    {
                        builder.Append(',');
                    }
                    Visit(assignExp, adapter, builder, appendParameter);
                    adapter.AppendColumnName(builder, binding.Member);
                    continue;
                }
                if (member.Member.IsNotMapped())
                {
                    continue;
                }
                if (index > 0 && index < init.Bindings.Count)
                {
                    builder.Append(',');
                }
                if (appendParameter)
                {
                    Visit(member.Expression, adapter, builder, true);
                }
                if (member.Member.Name != binding.Member.Name)
                {
                    adapter.AppendAliasColumnName(builder, member.Member, binding.Member);
                }
                else
                {
                    bool isAlias = adapter.AppendColumnName(builder, member.Member);
                    if (!isAlias) continue;
                    builder.Append(" AS ");
                    adapter.AppendQuoteName(builder, binding.Member.Name);
                }
            }
        }

        private static void VisitUnary(UnaryExpression u, ISqlAdapter adapter, StringBuilder builder, bool appendParameter)
        {
            Visit(u.Operand, adapter, builder, appendParameter);
        }

        private static void VisitMemberAccess(MemberExpression m, ISqlAdapter adapter, StringBuilder builder, bool appendParameter)
        {
            if (appendParameter)
            {
                Visit(m.Expression, adapter, builder, true);
            }
            adapter.AppendColumnName(builder, m.Member);
        }

        private static void VisitParameter(ParameterExpression m, ISqlAdapter adapter, StringBuilder builder)
        {
            builder.Append(adapter.GetQuoteName(m.Name)).Append('.');
        }
    }
}
