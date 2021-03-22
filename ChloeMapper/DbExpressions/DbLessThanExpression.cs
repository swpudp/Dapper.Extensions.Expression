using System.Reflection;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Extensions.Expression.DbExpressions
{
    public class DbLessThanExpression : DbBinaryExpression
    {
        public DbLessThanExpression(DbExpression left, DbExpression right)
            : this(left, right, null)
        {

        }
        public DbLessThanExpression(DbExpression left, DbExpression right, MethodInfo method)
            : base(DbExpressionType.LessThan, PublicConstants.TypeOfBoolean, left, right, method)
        {

        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

}
