using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Dapper.Extensions.Expression.Visitors
{
    /// <summary>
    /// 参数替换
    /// 将第一个参数替换成t1,第二个参数替换成t2...
    /// </summary>
    internal class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ReadOnlyCollection<ParameterExpression> _parameters;
        private static readonly IDictionary<RuntimeTypeHandle, IDictionary<int, ParameterExpression>> NewExpressions = new ConcurrentDictionary<RuntimeTypeHandle, IDictionary<int, ParameterExpression>>();

        private ReplaceParameterVisitor(ReadOnlyCollection<ParameterExpression> parameters)
        {
            _parameters = parameters;
        }

        internal static LambdaExpression Replace(System.Linq.Expressions.Expression ex, ReadOnlyCollection<ParameterExpression> parameterExpressions)
        {
            Debug.WriteLine($"new ReplaceParameterVisitor for ex.{ex.Type},parameterExpressions.{string.Join(",", parameterExpressions.Select(x => x.Name))}");
            return parameterExpressions.Count > 1 ? new ReplaceParameterVisitor(parameterExpressions).Visit(ex) as LambdaExpression : ex as LambdaExpression;
        }

        protected override System.Linq.Expressions.Expression VisitParameter(ParameterExpression node)
        {
            if (!NewExpressions.TryGetValue(node.Type.TypeHandle, out IDictionary<int, ParameterExpression> typeParameterExpressions))
            {
                typeParameterExpressions = new ConcurrentDictionary<int, ParameterExpression>();
            }
            int parameterIndex = _parameters.IndexOf(node);
            if (typeParameterExpressions.TryGetValue(parameterIndex, out ParameterExpression p))
            {
                return base.VisitParameter(p);
            }
            p = System.Linq.Expressions.Expression.Parameter(node.Type, "t" + (parameterIndex + 1));
            typeParameterExpressions[parameterIndex] = p;
            NewExpressions[node.Type.TypeHandle] = typeParameterExpressions;
            return base.VisitParameter(p);
        }
    }
}
