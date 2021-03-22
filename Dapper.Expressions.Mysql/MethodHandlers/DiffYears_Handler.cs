using Dapper.Extensions.Expression.DbExpressions;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Expressions.Mysql.MethodHandlers
{
    class DiffYears_Handler : IMethodHandler
    {
        public bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method.DeclaringType != PublicConstants.TypeOfSql)
                return false;

            return true;
        }
        public void Process(DbMethodCallExpression exp, SqlGenerator generator)
        {
            SqlGenerator.DbFunction_DATEDIFF(generator, "YEAR", exp.Arguments[0], exp.Arguments[1]);
        }
    }
}
