using Dapper.Extensions.Expression.DbExpressions;
using Dapper.Extensions.Expression.Reflection;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Expressions.Mysql.MethodHandlers
{
    class ToString_Handler : IMethodHandler
    {
        public bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Arguments.Count != 0)
            {
                return false;
            }

            if (exp.Object.Type == PublicConstants.TypeOfString)
            {
                return true;
            }

            if (!SqlGenerator.NumericTypes.ContainsKey(exp.Object.Type.GetUnderlyingType()))
            {
                return false;
            }

            return true;
        }
        public void Process(DbMethodCallExpression exp, SqlGenerator generator)
        {
            if (exp.Object.Type == PublicConstants.TypeOfString)
            {
                exp.Object.Accept(generator);
                return;
            }

            DbConvertExpression c = DbExpression.Convert(exp.Object, PublicConstants.TypeOfString);
            c.Accept(generator);
        }
    }
}
