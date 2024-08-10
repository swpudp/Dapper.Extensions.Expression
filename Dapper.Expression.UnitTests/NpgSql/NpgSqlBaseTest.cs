using Npgsql;
using System.Data;

namespace Dapper.Extensions.Expression.UnitTests.NpgSql
{
    public abstract class NpgSqlBaseTest : BaseTest
    {
        protected override IDbConnection CreateConnection()
        {
            string connectionString = "Host=localhost;Username=postgres;Password=kqMkb98QtDTapVGi;Database=dapper_exp";
            NpgsqlDataSourceBuilder dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            NpgsqlDataSource dataSource = dataSourceBuilder.Build();
            IDbConnection connection = dataSource.OpenConnection();
            return connection;
        }
    }
}
