using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
