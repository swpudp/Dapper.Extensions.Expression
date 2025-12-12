using System;
using System.Collections.Generic;
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
        /// 获取表名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetTableName(Type type);

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
        string GetQuoteName(MemberInfo memberInfo);

        /// <summary>
        /// Adds a column equality to a parameter.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="memberInfo">The column name.</param>
        /// <param name="name">The column name.</param>
        void AppendBinaryColumn(StringBuilder sb, MemberInfo memberInfo, out string name);

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
        void DateTimeAddMethod(MethodCallExpression e, string function, ISqlAdapter adapter, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter);

        /// <summary>
        /// 求字符串长度函数
        /// </summary>
        bool HandleStringLength(MemberExpression memberExpression, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter);

        string ParseBool(bool v);

        /// <summary>
        /// 最大参数个数
        /// </summary>
        int MaxParameterCount { get; }

        /// <summary>
        /// 访问联合表达式，如 a ?? 0
        /// </summary>
        /// <param name="e">表达式</param>
        /// <param name="builder">构建器</param>
        /// <param name="appendParameter">是否追加参数</param>
        /// <param name="action">访问表达式动作</param>
        void VisitCoalesce(BinaryExpression e, StringBuilder builder, Action<System.Linq.Expressions.Expression, ISqlAdapter, StringBuilder> action);

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="builder">参数容器</param>
        /// <param name="name">列名称</param>
        /// <param name="parameterName">参数名称</param>
        void AddBinaryParameter(StringBuilder builder, string colName, string parameterName);

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="builder">参数容器</param>
        /// <param name="name">参数名称</param>
        void AddParameter(StringBuilder builder, string name);

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="builder">参数容器</param>
        /// <param name="property">属性</param>
        void AddParameter(StringBuilder builder, PropertyInfo property);

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="builder">参数列表</param>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        void AddParameter(Dictionary<string, object> parameters, string name, object value);

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="builder">参数列表</param>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        void AddParameter(DynamicParameters parameters, string name, object value);

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="property"></param>
        /// <param name="v"></param>
        /// <param name="value"></param>
        void AddParameter(Dictionary<string, object> parameters, PropertyInfo property, int index, object value);

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="builder">参数容器</param>
        /// <param name="property">属性</param>
        void AddParameter(StringBuilder builder, PropertyInfo property, int index);
    }
}
