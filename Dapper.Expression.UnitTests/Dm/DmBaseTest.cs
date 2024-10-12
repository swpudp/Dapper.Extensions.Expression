using Dm;
using System.Data;

namespace Dapper.Extensions.Expression.UnitTests.Dm
{
    public abstract class DmBaseTest : BaseTest
    {
        protected override IDbConnection CreateConnection()
        {
            IDbConnection connection = new DmConnection("Server=125.68.186.183:5237;UserId=SYSADMIN;PWD=Dameng@8888");
            return connection;
        }
    }
}
