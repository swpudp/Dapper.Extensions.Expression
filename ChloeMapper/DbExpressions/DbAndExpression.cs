using System.Reflection;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Extensions.Expression.DbExpressions
{
    public class DbAndExpression : DbBinaryExpression
    {
        public DbAndExpression(DbExpression left, DbExpression right)
            : this(left, right, null)
        {

        }
        public DbAndExpression(DbExpression left, DbExpression right, MethodInfo method)
            : base(DbExpressionType.And, PublicConstants.TypeOfBoolean, left, right, method)
        {

        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

}
