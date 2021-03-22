using System;
using Dapper.Extensions.Expression.Query.QueryExpressions;

namespace Dapper.Extensions.Expression
{
    public interface IQuery
    {
        Type ElementType { get; }

        QueryExpression QueryExpression { get; }
    }
}
