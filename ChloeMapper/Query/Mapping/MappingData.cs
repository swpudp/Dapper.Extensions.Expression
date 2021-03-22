using Dapper.Extensions.Expression.DbExpressions;

namespace Dapper.Extensions.Expression.Query.Mapping
{
    public class MappingData
    {
        public MappingData()
        {
        }
        public IObjectActivatorCreator ObjectActivatorCreator { get; set; }
        public DbSqlQueryExpression SqlQuery { get; set; }
    }
}
