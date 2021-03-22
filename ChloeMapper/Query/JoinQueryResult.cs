using Dapper.Extensions.Expression.DbExpressions;
using Dapper.Extensions.Expression.Query.Model;

namespace Dapper.Extensions.Expression.Query
{
    public class JoinQueryResult
    {
        public IObjectModel ResultModel { get; set; }
        public DbJoinTableExpression JoinTable { get; set; }
    }
}
