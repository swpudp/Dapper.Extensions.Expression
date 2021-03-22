using Dapper.Extensions.Expression.Core;
using Dapper.Extensions.Expression.DbExpressions;
using Dapper.Extensions.Expression.Infrastructure;

namespace Dapper.Expressions.Mysql
{
    public class DbExpressionTranslator : IDbExpressionTranslator
    {
        public static readonly DbExpressionTranslator Instance = new DbExpressionTranslator();
        public DbCommandInfo Translate(DbExpression expression)
        {
            SqlGenerator generator = MySqlSqlGenerator.CreateInstance();
            expression = EvaluableDbExpressionTransformer.Transform(expression);
            expression.Accept(generator);

            DbCommandInfo dbCommandInfo = new DbCommandInfo();
            dbCommandInfo.Parameters = generator.Parameters;
            dbCommandInfo.CommandText = generator.SqlBuilder.ToSql();

            return dbCommandInfo;
        }
    }
}
