using Dapper.Extensions.Expression.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Dapper.Extensions.Expression.Adapters
{
    /// <summary>
    /// The database adapter abstract implement .
    /// </summary>
    internal abstract class AbstractSqlAdapter
    {
        public abstract int MaxParameterCount { get; }

        public abstract string ParameterPrefix { get; }

        public abstract string LeftQuote { get; }

        public abstract string RightQuote { get; }

        /// <summary>
        /// Adds the name of a column.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="memberInfo">The column name.</param>
        public bool AppendColumnName(StringBuilder sb, MemberInfo memberInfo, Type type = null)
        {
            string name = GetQuoteName(memberInfo, out bool isAlias, type);
            sb.Append(name);
            return isAlias;
        }

        /// <summary>
        /// Adds the name of a column.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="memberInfo">The column name.</param>
        /// <param name="aliasMemberInfo">别名</param>
        public void AppendAliasColumnName(StringBuilder sb, MemberInfo memberInfo, MemberInfo aliasMemberInfo)
        {
            string columnName = GetQuoteName(memberInfo, out _);
            sb.AppendFormat("{0} AS {1}{2}{3}", columnName, LeftQuote, aliasMemberInfo.Name, RightQuote);
        }

        public void AppendQuoteName(StringBuilder sb, string name)
        {
            sb.AppendFormat("{0}{1}{2}", LeftQuote, name, RightQuote);
        }

        /// <summary>
        /// Adds the name of a table.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        public string GetQuoteName(string tableName)
        {
            return $"{LeftQuote}{tableName}{RightQuote}";
        }

        public string GetTableName(Type type)
        {
            TableNamingAttribute namingAttribute = type.GetCustomAttribute<TableNamingAttribute>();
            TableAttribute tableAttr = type.GetCustomAttribute<TableAttribute>();
            string originTableName = tableAttr != null ? tableAttr.Name : type.Name;
            string tableName = namingAttribute == null ? originTableName : NamingUtils.GetName(namingAttribute.Policy, originTableName);
            return GetQuoteName(tableName);
        }

        /// <summary>
        /// Adds the name of a column.
        /// </summary>
        /// <param name="memberInfo">The column name.</param>
        /// <param name="isAlias"></param>
        public string GetQuoteName(MemberInfo memberInfo, out bool isAlias, Type type = null)
        {
            FieldNamingAttribute namingAttribute = NamingUtils.GetNamingAttribute(memberInfo);
            if (namingAttribute == null && type != null)
            {
                namingAttribute = type.GetCustomAttribute<FieldNamingAttribute>(true);
            }
            ColumnAttribute columnAttribute = memberInfo.GetCustomAttribute<ColumnAttribute>();
            if (columnAttribute == null)
            {
                isAlias = namingAttribute != null;
                return $"{LeftQuote}{(namingAttribute == null ? memberInfo.Name : NamingUtils.GetName(namingAttribute.Policy, memberInfo.Name))}{RightQuote}";
            }
            isAlias = true;
            return $"{LeftQuote}{(namingAttribute == null ? columnAttribute.Name : NamingUtils.GetName(namingAttribute.Policy, columnAttribute.Name))}{RightQuote}";
        }

        /// <summary>
        /// Adds a column equality to a parameter.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="memberInfo">The column name.</param>
        /// <param name="name"></param>
        public void AppendColumnNameEqualsValue(StringBuilder sb, MemberInfo memberInfo, out string name)
        {
            FieldNamingAttribute namingAttribute = NamingUtils.GetNamingAttribute(memberInfo);
            ColumnAttribute columnAttribute = memberInfo.GetCustomAttribute<ColumnAttribute>();
            if (columnAttribute == null)
            {
                name = namingAttribute == null ? memberInfo.Name : NamingUtils.GetName(namingAttribute.Policy, memberInfo.Name);
                sb.AppendFormat("{0}{1}{2} = {3}{4}", LeftQuote, name, RightQuote, ParameterPrefix, memberInfo.Name);
            }
            else
            {
                name = namingAttribute == null ? columnAttribute.Name : NamingUtils.GetName(namingAttribute.Policy, columnAttribute.Name);
                sb.AppendFormat("{0}{1}{2} = {3}{4}", LeftQuote, name, RightQuote, ParameterPrefix, columnAttribute.Name);
            }
        }

        public virtual string ParseBool(bool v)
        {
            return v ? "1" : "0";
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="builder">参数容器</param>
        /// <param name="name">参数名称</param>
        public void AddParameter(StringBuilder builder, string name)
        {
            builder.AppendFormat("{0}{1}", ParameterPrefix, name);
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="builder">参数容器</param>
        /// <param name="name">列名称</param>
        /// <param name="parameterName">参数名称</param>
        public void AddBinaryParameter(StringBuilder builder, string colName, string parameterName)
        {
            builder.AppendFormat("{0}{1}{2}={3}{4}", LeftQuote, colName, RightQuote, ParameterPrefix, parameterName);
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="builder">参数容器</param>
        /// <param name="name">参数名称</param>
        public void AddParameter(Dictionary<string, object> parameters, string name, object value)
        {
            parameters.Add(ParameterPrefix + name, value);
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="builder">参数容器</param>
        /// <param name="name">参数名称</param>
        public void AddParameter(DynamicParameters parameters, string name, object value)
        {
            parameters.Add(ParameterPrefix + name, value);
        }
    }
}
