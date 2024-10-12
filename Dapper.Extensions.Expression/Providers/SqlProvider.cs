using Dapper.Extensions.Expression.Adapters;
using System;
using System.Collections.Generic;
using System.Data;

namespace Dapper.Extensions.Expression.Providers
{
    internal static class SqlProvider
    {
        private static readonly Dictionary<string, ISqlAdapter> Adapters = new Dictionary<string, ISqlAdapter>
        {
            ["MySqlConnection"] = new MySqlAdapter(),
            ["NpgsqlConnection"] = new NpgSqlAdapter(),
            ["SqlConnection"] = new MsSqlAdapter(),
            ["OracleConnection"] = new OracleAdapter(),
            ["DmConnection"] = new DmSqlAdapter(),
        };

        internal static ISqlAdapter GetFormatter(IDbConnection connection)
        {
            if (!Adapters.TryGetValue(connection.GetType().Name, out ISqlAdapter adapter))
            {
                throw new NotSupportedException("不支持的数据库类型");
            }
            return adapter;
        }

        /// <summary>
        /// 连接类型对应sql语句
        /// </summary>
        internal static readonly IDictionary<JoinType, string> JoinTypeSqlCause = new Dictionary<JoinType, string>
        {
            [JoinType.Full] = "FULL LEFT",
            [JoinType.Left] = "LEFT JOIN",
            [JoinType.Inner] = "INNER JOIN",
            [JoinType.Right] = "RIGHT JOIN"
        };
    }
}
