using Dapper.Extensions.Expression.Adapters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Dapper.Extensions.Expression
{
    /// <summary>
    /// select表达式解析
    /// </summary>
    internal static class SelectExpressionResolver
    {
        public static void Visit(System.Linq.Expressions.Expression e, ISqlAdapter adapter, StringBuilder selectBuilder)
        {
            switch (e.NodeType)
            {
                case ExpressionType.Lambda:
                    VisitLambda((LambdaExpression)e, adapter, selectBuilder);
                    break;
                case ExpressionType.New:
                    VisitNew((NewExpression)e, adapter, selectBuilder);
                    break;
                case ExpressionType.MemberInit:
                    VisitMemberInit((MemberInitExpression)e, adapter, selectBuilder);
                    break;
                case ExpressionType.Convert:
                    VisitUnary((UnaryExpression)e, adapter, selectBuilder);
                    break;
                case ExpressionType.MemberAccess:
                    VisitMemberAccess((MemberExpression)e, adapter, selectBuilder);
                    break;
                default:
                    throw new NotSupportedException($"不支持{e.NodeType}");
            }
        }

        private static void VisitLambda(LambdaExpression lambda, ISqlAdapter adapter, StringBuilder selectBuilder)
        {
            Visit(lambda.Body, adapter, selectBuilder);
        }

        private static void VisitNew(NewExpression nex, ISqlAdapter adapter, StringBuilder selectBuilder)
        {
            IList<MemberInfo> memberInfos = Visit(nex.Arguments).ToList();
            foreach (MemberInfo alias in nex.Members)
            {
                int index = nex.Members.IndexOf(alias);
                MemberInfo memberInfo = memberInfos[index];
                if (memberInfo.IsDefined(typeof(NotMappedAttribute)))
                {
                    continue;
                }
                if (index > 0 && index < nex.Members.Count)
                {
                    selectBuilder.Append(",");
                }
                if (alias.Name == memberInfo.Name)
                {
                    adapter.AppendColumnName(selectBuilder, memberInfo);
                }
                else
                {
                    adapter.AppendAliasColumnName(selectBuilder, memberInfo, alias);
                }
            }
        }

        private static IEnumerable<MemberInfo> Visit(ReadOnlyCollection<System.Linq.Expressions.Expression> expressions)
        {
            foreach (System.Linq.Expressions.Expression e in expressions)
            {
                if (e is MemberExpression memberExpression)
                {
                    yield return memberExpression.Member;
                }
            }
        }

        private static void VisitMemberInit(MemberInitExpression init, ISqlAdapter adapter, StringBuilder selectBuilder)
        {
            foreach (MemberBinding binding in init.Bindings)
            {
                if (binding.Member.IsDefined(typeof(NotMappedAttribute)))
                {
                    continue;
                }
                int index = init.Bindings.IndexOf(binding);
                if (index > 0 && index < init.Bindings.Count)
                {
                    selectBuilder.Append(",");
                }
                adapter.AppendColumnName(selectBuilder, binding.Member);
            }
        }

        private static void VisitUnary(UnaryExpression u, ISqlAdapter adapter, StringBuilder selectBuilder)
        {
            Visit(u.Operand, adapter, selectBuilder);
        }

        private static void VisitMemberAccess(MemberExpression m, ISqlAdapter adapter, StringBuilder selectBuilder)
        {
            adapter.AppendColumnName(selectBuilder, m.Member);
        }
    }
}
