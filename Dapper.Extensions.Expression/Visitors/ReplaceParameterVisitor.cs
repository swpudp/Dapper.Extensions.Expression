using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace Dapper.Extensions.Expression.Visitors
{
    /// <summary>
    /// 参数替换
    /// 将第一个参数替换成t1,第二个参数替换成t2...
    /// </summary>
    internal class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly Dictionary<RuntimeTypeHandle, ParameterExpression> _newExpressions = new Dictionary<RuntimeTypeHandle, ParameterExpression>();

        private ReplaceParameterVisitor(ReadOnlyCollection<ParameterExpression> parameters)
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                _newExpressions[parameters[i].Type.TypeHandle] = System.Linq.Expressions.Expression.Parameter(parameters[i].Type, "t" + (i + 1));
            }
        }

        internal static LambdaExpression Replace(System.Linq.Expressions.Expression ex, ReadOnlyCollection<ParameterExpression> parameterExpressions)
        {
            Debug.WriteLine($"new ReplaceParameterVisitor for ex.{ex.Type},parameterExpressions.{string.Join(",", parameterExpressions.Select(x => x.Name))}");
            return new ReplaceParameterVisitor(parameterExpressions).Visit(ex) as LambdaExpression;
        }

        protected override System.Linq.Expressions.Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(_newExpressions[node.Type.TypeHandle]);
        }
    }
}
