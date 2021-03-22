using Dapper.Extensions.Expression.DbExpressions;
using Dapper.Extensions.Expression.Query.Model;
using Dapper.Extensions.Expression.Query.QueryExpressions;

namespace Dapper.Extensions.Expression.Query.QueryState
{
    class DistinctQueryState : SubQueryState
    {
        public DistinctQueryState(QueryModel queryModel)
            : base(queryModel)
        {
        }

        public override IQueryState Accept(SelectExpression exp)
        {
            IQueryState state = this.AsSubQueryState();
            return state.Accept(exp);
        }

        public override DbSqlQueryExpression CreateSqlQuery()
        {
            DbSqlQueryExpression sqlQuery = base.CreateSqlQuery();
            sqlQuery.IsDistinct = true;

            return sqlQuery;
        }
    }
}
