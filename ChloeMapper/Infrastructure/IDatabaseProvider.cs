using System.Data;

namespace Dapper.Extensions.Expression.Infrastructure
{
    public interface IDatabaseProvider
    {
        string DatabaseType { get; }
        IDbConnection CreateConnection();
        IDbExpressionTranslator CreateDbExpressionTranslator();
        string CreateParameterName(string name);
    }
}
