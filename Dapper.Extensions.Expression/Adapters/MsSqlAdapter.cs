﻿using Dapper.Extensions.Expression.Utilities;
using Dapper.Extensions.Expression.Visitors;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Dapper.Extensions.Expression.Adapters
{
    /// <summary>
    /// The MySQL database adapter.
    /// </summary>
    internal class MsSqlAdapter : ISqlAdapter
    {
        public int MaxParameterCount => 2000;

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
            sb.AppendFormat("{0} AS [{1}]", columnName, aliasMemberInfo.Name);
        }

        public void AppendQuoteName(StringBuilder sb, string name)
        {
            sb.AppendFormat("[{0}]", name);
        }

        /// <summary>
        /// Adds the name of a table.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        public string GetQuoteName(string tableName)
        {
            return $"[{tableName}]";
        }

        public string GetTableName(Type type)
        {
            string tableName;
            TableNamingAttribute namingAttribute = type.GetCustomAttribute<TableNamingAttribute>();
            if (namingAttribute != null)
            {
                tableName = NamingUtils.GetName(namingAttribute.Policy, type.Name);
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
        public string GetQuoteName(MemberInfo memberInfo, out bool isAlias, Type type = null)
        {
            //todo 疑问，是否能获取到真实类型
            FieldNamingAttribute namingAttribute = NamingUtils.GetNamingAttribute(memberInfo);
            if (namingAttribute == null && type != null)
            {
                namingAttribute = type.GetCustomAttribute<FieldNamingAttribute>(true);
            }
            if (namingAttribute != null)
            {
                isAlias = true;
                return $"[{NamingUtils.GetName(namingAttribute.Policy, memberInfo.Name)}]";
            }
            ColumnAttribute columnAttribute = memberInfo.GetCustomAttribute<ColumnAttribute>();
            if (columnAttribute == null)
            {
                isAlias = false;
                return $"[{memberInfo.Name}]";
            }
            isAlias = true;
            return $"[{columnAttribute.Name}]";
        }

        /// <summary>
        /// Adds a column equality to a parameter.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="memberInfo">The column name.</param>
        /// <param name="name"></param>
        public void AppendColumnNameEqualsValue(StringBuilder sb, MemberInfo memberInfo, out string name)
        {
            //todo 疑问，是否能获取到真实类型
            FieldNamingAttribute namingAttribute = NamingUtils.GetNamingAttribute(memberInfo);
            if (namingAttribute != null)
            {
                name = NamingUtils.GetName(namingAttribute.Policy, memberInfo.Name);
                sb.AppendFormat("[{0}] = @{1}", name, memberInfo.Name);
                return;
            }
            ColumnAttribute columnAttribute = memberInfo.GetCustomAttribute<ColumnAttribute>();
            if (columnAttribute == null)
            {
                sb.AppendFormat("[{0}] = @{1}", memberInfo.Name, memberInfo.Name);
                name = memberInfo.Name;
            }
            else
            {
                sb.AppendFormat("[{0}] = @{1}", columnAttribute.Name, columnAttribute.Name);
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
            sb.AppendFormat(" OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", currentPage * pageSize, pageSize);
        }

        public void HandleDateTime(MemberExpression exp, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            MemberInfo member = exp.Member;
            if (member == ConstantDefined.PropertyDateTimeNow)
            {
                sqlBuilder.Append("GETDATE()");
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeUtcNow)
            {
                sqlBuilder.Append("GETUTCDATE()");
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeToday)
            {
                sqlBuilder.Append("CONVERT(date, GETDATE())");
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeDate)
            {
                sqlBuilder.Append("CONVERT(date,");
                WhereExpressionVisitor.InternalVisit(exp.Expression, this, sqlBuilder, parameters, appendParameter);
                sqlBuilder.Append(')');
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeYear)
            {
                AppendDatePart(exp, sqlBuilder, parameters, "DATEPART(YEAR,", appendParameter);
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeMonth)
            {
                AppendDatePart(exp, sqlBuilder, parameters, "DATEPART(MONTH,", appendParameter);
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeDay)
            {
                AppendDatePart(exp, sqlBuilder, parameters, "DATEPART(DAY,", appendParameter);
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeHour)
            {
                AppendDatePart(exp, sqlBuilder, parameters, "DATEPART(HOUR,", appendParameter);
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeMinute)
            {
                AppendDatePart(exp, sqlBuilder, parameters, "DATEPART(HOUR,", appendParameter);
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeSecond)
            {
                AppendDatePart(exp, sqlBuilder, parameters, "DATEPART(SECOND,", appendParameter);
                return;
            }
            /* MySql is not supports MILLISECOND */
            if (member == ConstantDefined.PropertyDateTimeDayOfWeek)
            {
                sqlBuilder.Append('(');
                AppendDatePart(exp, sqlBuilder, parameters, "DATEPART(WEEKDAY,", appendParameter);
                sqlBuilder.Append(" - 1)");
            }
        }

        private void AppendDatePart(MemberExpression exp, StringBuilder sqlBuilder, DynamicParameters parameters, string functionName, bool appendParameter)
        {
            sqlBuilder.Append(functionName);
            sqlBuilder.Append('(');
            WhereExpressionVisitor.InternalVisit(exp.Expression, this, sqlBuilder, parameters, appendParameter);
            sqlBuilder.Append(')');
        }

        /// <summary>
        /// 处理日期
        /// </summary>
        public void DateTimeAddMethod(MethodCallExpression e, string function, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            sqlBuilder.AppendFormat("DateAdd({0},", function);
            WhereExpressionVisitor.InternalVisit(e.Arguments[0], this, sqlBuilder, parameters, appendParameter);
            sqlBuilder.Append(',');
            WhereExpressionVisitor.InternalVisit(e.Object, this, sqlBuilder, parameters, appendParameter);
            sqlBuilder.Append(')');
        }

        public bool HandleStringLength(MemberExpression memberExpression, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            if (memberExpression.Member.Name != "Length")
            {
                return false;
            }
            sqlBuilder.Append("LEN(");
            WhereExpressionVisitor.InternalVisit(memberExpression.Expression, this, sqlBuilder, parameters, appendParameter);
            sqlBuilder.Append(')');
            return true;
        }


        public string ParseBool(bool v)
        {
            return v ? "1" : "0";
        }

        public void VisitCoalesce(BinaryExpression e, StringBuilder builder, bool appendParameter, Action<System.Linq.Expressions.Expression, ISqlAdapter, StringBuilder, bool> action)
        {
            builder.Append("CAST(ISNULL(");
            //参数部分
            action(e.Left, this, builder, appendParameter);
            builder.Append(',');
            //值部分
            action(e.Right, this, builder, appendParameter);
            builder.Append(") AS CHAR) AS ");
        }
    }
}