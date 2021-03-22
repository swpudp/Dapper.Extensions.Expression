using System;
using System.Data;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Extensions.Expression.Infrastructure
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }

    public class DbConnectionFactory : IDbConnectionFactory
    {
        Func<IDbConnection> _connectionCreator;
        public DbConnectionFactory(Func<IDbConnection> connectionCreator)
        {
            PublicHelper.CheckNull(connectionCreator);
            this._connectionCreator = connectionCreator;
        }

        public IDbConnection CreateConnection()
        {
            return this._connectionCreator();
        }
    }
}
