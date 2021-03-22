using Dapper.Extensions.Expression.DbExpressions;

namespace Dapper.Expressions.Mysql
{
    interface IMethodHandler
    {
        bool CanProcess(DbMethodCallExpression exp);
        void Process(DbMethodCallExpression exp, SqlGenerator generator);
    }
}
