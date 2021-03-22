using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Dapper.Extensions.Expression
{
    /// <summary>
    /// select表达式解析
    /// </summary>
    internal static class OrderExpressionResolver
    {
        public static IEnumerable<MemberInfo> Visit(System.Linq.Expressions.Expression e)
        {
            switch (e.NodeType)
            {
                case ExpressionType.Lambda:
                    return VisitLambda((LambdaExpression)e);
                case ExpressionType.New:
                    return VisitNew((NewExpression)e);
                case ExpressionType.MemberInit:
                    return VisitMemberInit((MemberInitExpression)e);
                case ExpressionType.Convert:
                    return VisitUnary((UnaryExpression)e);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression)e);
                default:
                    throw new NotSupportedException($"不支持{e.NodeType}");
            }
        }

        private static IEnumerable<MemberInfo> VisitLambda(LambdaExpression lambda)
        {
            return Visit(lambda.Body);
        }

        private static IEnumerable<MemberInfo> VisitNew(NewExpression nex)
        {
            foreach (MemberInfo alias in nex.Members)
            {
                if (alias.IsDefined(typeof(NotMappedAttribute)))
                {
                    continue;
                }
                yield return alias;
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

        private static IEnumerable<MemberInfo> VisitMemberInit(MemberInitExpression init)
        {
            foreach (MemberBinding binding in init.Bindings)
            {
                if (binding.Member.IsDefined(typeof(NotMappedAttribute)))
                {
                    continue;
                }
                yield return binding.Member;
            }
        }

        private static IEnumerable<MemberInfo> VisitUnary(UnaryExpression u)
        {
            return Visit(u.Operand);
        }

        private static IEnumerable<MemberInfo> VisitMemberAccess(MemberExpression m)
        {
            yield return m.Member;
        }
    }
}
