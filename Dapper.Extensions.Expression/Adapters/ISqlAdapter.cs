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
        void AppendColumnName(StringBuilder sb, MemberInfo memberInfo);

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
        /// <param name="tableName">The table name.</param>
        void AppendTableName(StringBuilder sb, string tableName);

        /// <summary>
        /// Adds the quote of a table or column.
        /// </summary>
        /// <param name="name">The table or column name.</param>
        string GetQuoteName(string name);

        /// <summary>
        /// Adds the name of a column.
        /// </summary>
        /// <param name="memberInfo">The column name.</param>
        string GetQuoteName(MemberInfo memberInfo);

        /// <summary>
        /// Adds a column equality to a parameter.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="memberInfo">The column name.</param>
        void AppendColumnNameEqualsValue(StringBuilder sb, MemberInfo memberInfo);

        /// <summary>
        /// 增加分页信息
        /// </summary>
        void AppendPage(StringBuilder sb, int page, int pageSize);

        /// <summary>
        /// 处理日期
        /// </summary>
        void HandleDateTime(ExpressionVisitor visitor, MemberExpression memberExpression, StringBuilder sqlBuilder, DynamicParameters parameters);

        /// <summary>
        /// 处理日期
        /// </summary>
        void DateTimeAddMethod(ExpressionVisitor visitor, MethodCallExpression e, string function, StringBuilder sqlBuilder, DynamicParameters parameters);

        /// <summary>
        /// 求字符串长度函数
        /// </summary>
        /// <param name="memberExpression"></param>
        /// <param name="visitor"></param>
        /// <param name="sqlBuilder"></param>
        /// <param name="parameters"></param>
        bool HandleStringLength(MemberExpression memberExpression, ExpressionVisitor visitor, StringBuilder sqlBuilder, DynamicParameters parameters);
    }
}
