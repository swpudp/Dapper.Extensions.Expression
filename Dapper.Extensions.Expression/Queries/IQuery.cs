using System.Data;

namespace Dapper.Extensions.Expression.Queries
{
    public interface IQuery
    {
        /// <summary>
        /// 参数
        /// </summary>
        DynamicParameters Parameters { get; }

        /// <summary>
        /// 数据库连接
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// 获取SQL文本
        /// </summary>
        /// <returns></returns>
        string GetCommandText();

        /// <summary>
        /// 获取COUNT SQL文本
        /// </summary>
        /// <returns></returns>
        string GetCountCommandText();
    }
}
