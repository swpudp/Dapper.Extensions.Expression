using System.Linq.Expressions;
using Dapper.Extensions.Expression.Core.Visitors;
using Dapper.Extensions.Expression.DbExpressions;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Extensions.Expression.Query.Visitors
{
    class FilterPredicateParser : ExpressionVisitor<DbExpression>
    {
        public static DbExpression Parse(LambdaExpression filterPredicate, ScopeParameterDictionary scopeParameters, StringSet scopeTables)
        {
            return GeneralExpressionParser.Parse(BooleanResultExpressionTransformer.Transform(filterPredicate), scopeParameters, scopeTables);
        }
    }
}
