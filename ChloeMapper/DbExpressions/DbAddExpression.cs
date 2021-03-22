using System;
using System.Reflection;

namespace Dapper.Extensions.Expression.DbExpressions
{
    public class DbAddExpression : DbBinaryExpression
    {
        public DbAddExpression(Type type, DbExpression left, DbExpression right, MethodInfo method)
            : base(DbExpressionType.Add, type, left, right, method)
        {

        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
