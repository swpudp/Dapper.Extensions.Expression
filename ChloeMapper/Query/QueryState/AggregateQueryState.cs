using Dapper.Extensions.Expression.Query.Model;

namespace Dapper.Extensions.Expression.Query.QueryState
{
    class AggregateQueryState : QueryStateBase, IQueryState
    {
        public AggregateQueryState(QueryModel queryModel)
            : base(queryModel)
        {
        }
    }
}
