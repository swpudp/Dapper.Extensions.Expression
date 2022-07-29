using Dapper.Extensions.Expression.Adapters;
using System;
using System.Collections.Generic;
using System.Data;

namespace Dapper.Extensions.Expression.Providers
{
    internal static class SqlProvider
    {
        private static readonly IDictionary<string, ISqlAdapter> Adapters = new Dictionary<string, ISqlAdapter>
        {
            ["mysqlconnection_" + NamingPolicy.None] = new MySqlAdapter(NamingPolicy.None),
            ["mysqlconnection_" + NamingPolicy.CamelCase] = new MySqlAdapter(NamingPolicy.CamelCase),
            ["mysqlconnection_" + NamingPolicy.LowerCase] = new MySqlAdapter(NamingPolicy.LowerCase),
            ["mysqlconnection_" + NamingPolicy.SnakeCase] = new MySqlAdapter(NamingPolicy.SnakeCase),
            ["mysqlconnection_" + NamingPolicy.UpperSnakeCase] = new MySqlAdapter(NamingPolicy.UpperSnakeCase),
            ["mysqlconnection_" + NamingPolicy.UpperCase] = new MySqlAdapter(NamingPolicy.UpperCase)
        };

        internal static ISqlAdapter GetFormatter(IDbConnection connection, NamingPolicy namingPolicy)
        {
            string name = $"{connection.GetType().Name.ToLower()}_{namingPolicy}";
            if (!Adapters.TryGetValue(name, out ISqlAdapter adapter))
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
