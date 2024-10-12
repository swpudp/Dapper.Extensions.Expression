using System.Data;
using Microsoft.Data.SqlClient;

namespace Dapper.Extensions.Expression.UnitTests.MsSql
{
    public abstract class MsSqlBaseTest : BaseTest
    {
        protected override IDbConnection CreateConnection()
        {
            IDbConnection connection = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=E:\\projects\\Dapper.Extensions.Expression\\database\\Dapper_Exp.mdf;Integrated Security=True;Connect Timeout=30");
            return connection;
        }
    }
}
