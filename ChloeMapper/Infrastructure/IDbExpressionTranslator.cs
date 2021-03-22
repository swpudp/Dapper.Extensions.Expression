using Dapper.Extensions.Expression.Core;
using Dapper.Extensions.Expression.DbExpressions;

namespace Dapper.Extensions.Expression.Infrastructure
{
    public interface IDbExpressionTranslator
    {
        DbCommandInfo Translate(DbExpression expression);
    }
}
