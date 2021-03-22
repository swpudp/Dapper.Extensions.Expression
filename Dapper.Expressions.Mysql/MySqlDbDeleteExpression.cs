﻿using Dapper.Extensions.Expression.DbExpressions;

namespace Dapper.Expressions.Mysql
{
    class MySqlDbDeleteExpression : DbDeleteExpression
    {
        public MySqlDbDeleteExpression(DbTable table)
          : this(table, null)
        {
        }
        public MySqlDbDeleteExpression(DbTable table, DbExpression condition)
            : base(table, condition)
        {
        }

        public int? Limits { get; set; }
    }
}
