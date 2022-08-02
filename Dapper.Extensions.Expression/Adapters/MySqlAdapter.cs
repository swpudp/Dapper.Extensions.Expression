using Dapper.Extensions.Expression.Extensions;
using Dapper.Extensions.Expression.Utilities;
using Dapper.Extensions.Expression.Visitors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Dapper.Extensions.Expression.Adapters
{
    /// <summary>
    /// The MySQL database adapter.
    /// </summary>
    internal class MySqlAdapter : ISqlAdapter
    {
        private readonly NamingPolicy _namingPolicy;
        private readonly Func<string, string> _namingPolicyHandler;

        /// <summary>
        /// 命名策略
        /// </summary>
        /// <param name="namingPolicy"></param>
        public MySqlAdapter(NamingPolicy namingPolicy)
        {
            _namingPolicy = namingPolicy;
            _namingPolicyHandler = NamingUtils.NamingPolicyHandlers[namingPolicy];
        }

        /// <summary>
        /// Adds the name of a column.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="memberInfo">The column name.</param>
        public bool AppendColumnName(StringBuilder sb, MemberInfo memberInfo)
        {
            string name = GetQuoteName(memberInfo, out bool isAlias);
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
            sb.AppendFormat("{0} AS `{1}`", columnName, aliasMemberInfo.Name);
        }

        public void AppendQuoteName(StringBuilder sb, string name)
        {
            sb.AppendFormat("`{0}`", name);
        }

        /// <summary>
        /// Adds the name of a table.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        public string GetQuoteName(string tableName)
        {
            return $"`{tableName}`";
        }

        public string GetTableName(Type type)
        {
            string tableName = null;
            if (_namingPolicy != NamingPolicy.None)
            {
                tableName = (_namingPolicyHandler(type.Name));
            }
            else
            {
                TableAttribute tableAttr = type.GetCustomAttribute<TableAttribute>();
                tableName = tableAttr != null ? tableAttr.Name : type.Name;
            }
            return GetQuoteName(tableName);
        }

        /// <summary>
        /// Adds the name of a column.
        /// </summary>
        /// <param name="memberInfo">The column name.</param>
        /// <param name="isAlias"></param>
        public string GetQuoteName(MemberInfo memberInfo, out bool isAlias)
        {
            if (_namingPolicy != NamingPolicy.None)
            {
                isAlias = true;
                return $"`{_namingPolicyHandler(memberInfo.Name)}`";
            }
            ColumnAttribute columnAttribute = memberInfo.GetCustomAttribute<ColumnAttribute>();
            if (columnAttribute == null)
            {
                isAlias = false;
                return $"`{memberInfo.Name}`";
            }
            isAlias = true;
            return $"`{columnAttribute.Name}`";
        }

        /// <summary>
        /// Adds a column equality to a parameter.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="memberInfo">The column name.</param>
        /// <param name="name"></param>
        public void AppendColumnNameEqualsValue(StringBuilder sb, MemberInfo memberInfo, out string name)
        {
            if (_namingPolicy != NamingPolicy.None)
            {
                name = _namingPolicyHandler(memberInfo.Name);
                sb.AppendFormat("`{0}` = @{1}", name, memberInfo.Name);
                return;
            }
            ColumnAttribute columnAttribute = memberInfo.GetCustomAttribute<ColumnAttribute>();
            if (columnAttribute == null)
            {
                sb.AppendFormat("`{0}` = @{1}", memberInfo.Name, memberInfo.Name);
                name = memberInfo.Name;
            }
            else
            {
                sb.AppendFormat("`{0}` = @{1}", columnAttribute.Name, columnAttribute.Name);
                name = columnAttribute.Name;
            }
        }

        /// <summary>
        /// 增加分页信息
        /// </summary>
        public void AppendPage(StringBuilder sb, int page, int pageSize)
        {
            if (page == 0 && pageSize == 0)
            {
                return;
            }
            int currentPage = page > 1 ? page - 1 : 0;
            sb.AppendFormat(" LIMIT {0},{1}", currentPage * pageSize, pageSize);
        }

        public void HandleDateTime(MemberExpression exp, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            MemberInfo member = exp.Member;
            if (member == ConstantDefined.PropertyDateTimeNow)
            {
                sqlBuilder.Append("NOW()");
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeUtcNow)
            {
                sqlBuilder.Append("UTC_TIMESTAMP()");
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeToday)
            {
                sqlBuilder.Append("CURDATE()");
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeDate)
            {
                sqlBuilder.Append("DATE(");
                WhereExpressionVisitor.InternalVisit(exp.Expression, this, sqlBuilder, parameters, appendParameter);
                sqlBuilder.Append(")");
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeYear)
            {
                AppendDatePart(exp, sqlBuilder, parameters, "YEAR", appendParameter);
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeMonth)
            {
                AppendDatePart(exp, sqlBuilder, parameters, "MONTH", appendParameter);
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeDay)
            {
                AppendDatePart(exp, sqlBuilder, parameters, "DAY", appendParameter);
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeHour)
            {
                AppendDatePart(exp, sqlBuilder, parameters, "HOUR", appendParameter);
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeMinute)
            {
                AppendDatePart(exp, sqlBuilder, parameters, "HOUR", appendParameter);
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeSecond)
            {
                AppendDatePart(exp, sqlBuilder, parameters, "SECOND", appendParameter);
                return;
            }
            /* MySql is not supports MILLISECOND */
            if (member == ConstantDefined.PropertyDateTimeDayOfWeek)
            {
                sqlBuilder.Append("(");
                AppendDatePart(exp, sqlBuilder, parameters, "DAYOFWEEK", appendParameter);
                sqlBuilder.Append(" - 1)");
            }
        }

        private void AppendDatePart(MemberExpression exp, StringBuilder sqlBuilder, DynamicParameters parameters, string functionName, bool appendParameter)
        {
            sqlBuilder.Append(functionName);
            sqlBuilder.Append("(");
            WhereExpressionVisitor.InternalVisit(exp.Expression, this, sqlBuilder, parameters, appendParameter);
            sqlBuilder.Append(")");
        }

        /// <summary>
        /// 处理日期
        /// </summary>
        public void DateTimeAddMethod(MethodCallExpression e, string function, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            sqlBuilder.Append("DATE_ADD(");
            WhereExpressionVisitor.InternalVisit(e.Object, this, sqlBuilder, parameters, appendParameter);
            sqlBuilder.Append(",INTERVAL ");
            WhereExpressionVisitor.InternalVisit(e.Arguments[0], this, sqlBuilder, parameters, appendParameter);
            sqlBuilder.AppendFormat(" {0} ", function);
            sqlBuilder.Append(")");
        }

        public bool HandleStringLength(MemberExpression memberExpression, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            if (memberExpression.Member.Name != "Length")
            {
                return false;
            }
            sqlBuilder.Append("LENGTH(");
            WhereExpressionVisitor.InternalVisit(memberExpression.Expression, this, sqlBuilder, parameters, appendParameter);
            sqlBuilder.Append(")");
            return true;
        }
    }
}
