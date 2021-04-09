using System.Linq.Expressions;

namespace Dapper.Extensions.Expression.Extensions
{
    internal static class ExpressionExtension
    {
        internal static System.Linq.Expressions.Expression StripQuotes(this System.Linq.Expressions.Expression exp)
        {
            while (exp.NodeType == ExpressionType.Quote)
            {
                exp = ((UnaryExpression)exp).Operand;
            }
            return exp;
        }
    }
}
