using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
