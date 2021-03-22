using System.Linq;
using Dapper.Extensions.Expression.DbExpressions;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Expressions.Mysql.MethodHandlers
{
    class Sum_Handler : IMethodHandler
    {
        public bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method.DeclaringType != PublicConstants.TypeOfSql)
                return false;

            return true;
        }
        public void Process(DbMethodCallExpression exp, SqlGenerator generator)
        {
            SqlGenerator.Aggregate_Sum(generator, exp.Arguments.First(), exp.Method.ReturnType);
        }
    }
}
