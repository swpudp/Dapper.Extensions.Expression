using Dapper.Extensions.Expression.DbExpressions;
using Dapper.Extensions.Expression.Utility;
using System.Linq;

namespace Dapper.Expressions.Mysql.MethodHandlers
{
    class EndsWith_Handler : IMethodHandler
    {
        public bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method != PublicConstants.MethodInfo_String_EndsWith)
                return false;

            return true;
        }
        public void Process(DbMethodCallExpression exp, SqlGenerator generator)
        {
            exp.Object.Accept(generator);

            generator.SqlBuilder.Append(" LIKE ");
            generator.SqlBuilder.Append("CONCAT(");
            generator.SqlBuilder.Append("'%',");
            exp.Arguments.First().Accept(generator);
            generator.SqlBuilder.Append(")");
        }
    }

}
