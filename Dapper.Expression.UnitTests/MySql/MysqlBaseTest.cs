using MySql.Data.MySqlClient;
using System.Data;

namespace Dapper.Extensions.Expression.UnitTests.MySql
{
    public abstract class MysqlBaseTest : BaseTest
    {
        protected override IDbConnection CreateConnection()
        {
            //IDbConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;database=dapper_exp;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8");
            IDbConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;database=dapper_exp;uid=root;pwd=Q1@we34r;charset=utf8");
            return connection;
        }
    }
}
