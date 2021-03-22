using System.Reflection;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Extensions.Expression.DbExpressions
{
    public class DbGreaterThanOrEqualExpression : DbBinaryExpression
    {
        public DbGreaterThanOrEqualExpression(DbExpression left, DbExpression right)
            : this(left, right, null)
        {

        }
        public DbGreaterThanOrEqualExpression(DbExpression left, DbExpression right, MethodInfo method)
            : base(DbExpressionType.GreaterThanOrEqual, PublicConstants.TypeOfBoolean, left, right, method)
        {

        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

}
