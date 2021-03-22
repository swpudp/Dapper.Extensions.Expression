using Dapper.Extensions.Expression.DbExpressions;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Expressions.Mysql.MethodHandlers
{
    class DiffMilliseconds_Handler : IMethodHandler
    {
        public bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method.DeclaringType != PublicConstants.TypeOfSql)
                return false;

            return false;
        }
        public void Process(DbMethodCallExpression exp, SqlGenerator generator)
        {
            throw UtilExceptions.NotSupportedMethod(exp.Method);
        }
    }
}
