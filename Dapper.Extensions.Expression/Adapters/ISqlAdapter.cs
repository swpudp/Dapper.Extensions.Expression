using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Dapper.Extensions.Expression.Adapters
{
    /// <summary>
    /// The interface for all Dapper.Contrib database operations
    /// Implementing this is each provider's model.
    /// </summary>
    internal interface ISqlAdapter
    {
        /// <summary>
        /// Adds the name of a column.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="memberInfo">The column name.</param>
        bool AppendColumnName(StringBuilder sb, MemberInfo memberInfo);

        /// <summary>
        /// Adds the name of a column.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="memberInfo">The column name.</param>
        /// <param name="aliasMemberInfo">别名</param>
        void AppendAliasColumnName(StringBuilder sb, MemberInfo memberInfo, MemberInfo aliasMemberInfo);

        /// <summary>
        /// Adds the name of a table.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="name">The table name.</param>
        void AppendQuoteName(StringBuilder sb, string name);

        /// <summary>
        /// Adds the quote of a table or column.
        /// </summary>
        /// <param name="name">The table or column name.</param>
        string GetQuoteName(string name);

        /// <summary>
        /// Adds the name of a column.
        /// </summary>
        /// <param name="memberInfo">The column name.</param>
        /// <param name="isAlias">别名</param>
        string GetQuoteName(MemberInfo memberInfo, out bool isAlias);

        /// <summary>
        /// Adds a column equality to a parameter.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="memberInfo">The column name.</param>
        /// <param name="name">The column name.</param>
        void AppendColumnNameEqualsValue(StringBuilder sb, MemberInfo memberInfo, out string name);

        /// <summary>
        /// 增加分页信息
        /// </summary>
        void AppendPage(StringBuilder sb, int page, int pageSize);

        /// <summary>
        /// 处理日期
        /// </summary>
        void HandleDateTime(MemberExpression memberExpression, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter);

        /// <summary>
        /// 处理日期
        /// </summary>
        void DateTimeAddMethod(MethodCallExpression e, string function, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter);

        /// <summary>
        /// 求字符串长度函数
        /// </summary>
        bool HandleStringLength(MemberExpression memberExpression, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter);

        /// <summary>
        /// 命名策略
        /// </summary>
        NamingPolicy NamingPolicy { get; }
    }
}
