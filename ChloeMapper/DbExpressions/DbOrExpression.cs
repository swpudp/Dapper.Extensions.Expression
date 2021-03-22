using System.Reflection;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Extensions.Expression.DbExpressions
{
    public class DbOrExpression : DbBinaryExpression
    {
        public DbOrExpression(DbExpression left, DbExpression right)
            : this(left, right, null)
        {
        }
        public DbOrExpression(DbExpression left, DbExpression right, MethodInfo method)
            : base(DbExpressionType.Or, PublicConstants.TypeOfBoolean, left, right, method)
        {
        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

}
