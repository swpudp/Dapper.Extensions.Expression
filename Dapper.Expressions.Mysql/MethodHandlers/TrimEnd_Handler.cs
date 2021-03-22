using Dapper.Extensions.Expression.DbExpressions;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Expressions.Mysql.MethodHandlers
{
    class TrimEnd_Handler : IMethodHandler
    {
        public bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method != PublicConstants.MethodInfo_String_TrimEnd)
                return false;

            return true;
        }
        public void Process(DbMethodCallExpression exp, SqlGenerator generator)
        {
            MethodHandlerHelper.EnsureTrimCharArgumentIsSpaces(exp.Arguments[0]);

            generator.SqlBuilder.Append("RTRIM(");
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(")");
        }
    }
}
