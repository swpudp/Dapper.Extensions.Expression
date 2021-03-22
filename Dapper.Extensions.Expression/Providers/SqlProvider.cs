using System;
using System.Collections.Generic;
using System.Data;
using Dapper.Extensions.Expression.Adapters;

namespace Dapper.Extensions.Expression.Providers
{
    internal static class SqlProvider
    {
        private static readonly IDictionary<string, ISqlAdapter> Adapters = new Dictionary<string, ISqlAdapter>
        {
            ["mysqlconnection"] = new MySqlAdapter()
        };

        internal static ISqlAdapter GetFormatter(IDbConnection connection)
        {
            string name = connection.GetType().Name.ToLower();
            if (!Adapters.TryGetValue(name, out ISqlAdapter adapter))
            {
                throw new NotSupportedException("不支持的数据库类型");
            }
            return adapter;
        }
    }
}
