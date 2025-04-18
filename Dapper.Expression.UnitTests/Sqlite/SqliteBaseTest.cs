using Microsoft.Data.Sqlite;
using System.Data;

namespace Dapper.Extensions.Expression.UnitTests.Sqlite
{
    public abstract class SqliteBaseTest : BaseTest
    {
        protected override IDbConnection CreateConnection()
        {
            string connectionString = "Data Source=dapper_exp.db";
            string fullConnectionString = new SqliteConnectionStringBuilder(connectionString)
            {
                Mode=SqliteOpenMode.ReadWriteCreate
            }.ToString();
            return new SqliteConnection(fullConnectionString);
        }
    }
}
