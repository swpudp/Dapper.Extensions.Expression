using System.Linq.Expressions;

namespace Dapper.Extensions.Expression.Core.Visitors
{
    public class ParameterExpressionReplacer : ExpressionVisitor
    {
        ParameterExpression _replaceWith;

        ParameterExpressionReplacer(ParameterExpression replaceWith)
        {
            this._replaceWith = replaceWith;
        }

        public static System.Linq.Expressions.Expression Replace(System.Linq.Expressions.Expression expression, ParameterExpression replaceWith)
        {
            return new ParameterExpressionReplacer(replaceWith).Visit(expression);
        }

        protected override System.Linq.Expressions.Expression VisitParameter(ParameterExpression node)
        {
            return this._replaceWith;
        }
    }

}
