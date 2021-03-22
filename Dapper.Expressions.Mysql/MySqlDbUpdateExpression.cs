using Dapper.Extensions.Expression.DbExpressions;

namespace Dapper.Expressions.Mysql
{
    class MySqlDbUpdateExpression : DbUpdateExpression
    {
        public MySqlDbUpdateExpression(DbTable table)
          : this(table, null)
        {
        }
        public MySqlDbUpdateExpression(DbTable table, DbExpression condition)
            : base(table, condition)
        {
        }

        public int? Limits { get; set; }
    }
}
