using Dapper.Extensions.Expression.DbExpressions;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Expressions.Mysql.MethodHandlers
{
    class DiffHours_Handler : IMethodHandler
    {
        public bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method.DeclaringType != PublicConstants.TypeOfSql)
                return false;

            return true;
        }
        public void Process(DbMethodCallExpression exp, SqlGenerator generator)
        {
            SqlGenerator.DbFunction_DATEDIFF(generator, "HOUR", exp.Arguments[0], exp.Arguments[1]);
        }
    }
}
