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
        /// <summary>
        /// Adds the name of a column.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="memberInfo">The column name.</param>
        public void AppendColumnName(StringBuilder sb, MemberInfo memberInfo)
        {
            string name = GetQuoteName(memberInfo);
            sb.Append(name);
        }

        /// <summary>
        /// Adds the name of a column.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="memberInfo">The column name.</param>
        /// <param name="aliasMemberInfo">别名</param>
        public void AppendAliasColumnName(StringBuilder sb, MemberInfo memberInfo, MemberInfo aliasMemberInfo)
        {
            sb.AppendFormat("`{0}` AS `{1}`", memberInfo.Name, aliasMemberInfo.Name);
        }

        public void AppendTableName(StringBuilder sb, string tableName)
        {
            sb.AppendFormat("`{0}`", tableName);
        }

        /// <summary>
        /// Adds the name of a table.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        public string GetQuoteName(string tableName)
        {
            return $"`{tableName}`";
        }

        /// <summary>
        /// Adds the name of a column.
        /// </summary>
        /// <param name="memberInfo">The column name.</param>
        public string GetQuoteName(MemberInfo memberInfo)
        {
            if (!memberInfo.IsDefined(typeof(ColumnAttribute)))
            {
                return $"`{memberInfo.Name}`";
            }
            ColumnAttribute columnAttribute = memberInfo.GetCustomAttribute<ColumnAttribute>();
            return $"`{columnAttribute.ColumnName}`";
        }

        /// <summary>
        /// Adds a column equality to a parameter.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="memberInfo">The column name.</param>
        public void AppendColumnNameEqualsValue(StringBuilder sb, MemberInfo memberInfo)
        {
            if (memberInfo.IsDefined(typeof(ColumnAttribute)))
            {
                ColumnAttribute columnAttribute = memberInfo.GetCustomAttribute<ColumnAttribute>();
                sb.AppendFormat("`{0}` = @{1}", columnAttribute.ColumnName, columnAttribute.ColumnName);
            }
            else
            {
                sb.AppendFormat("`{0}` = @{1}", memberInfo.Name, memberInfo.Name);
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
            int currentPage = page - 1 <= 0 ? 0 : page - 1;
            sb.AppendFormat("LIMIT {0},{1}", currentPage * pageSize, pageSize);
        }

        public void HandleDateTime(ExpressionVisitor visitor, MemberExpression exp, StringBuilder sqlBuilder, DynamicParameters parameters)
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
                visitor.InternalVisit(exp.Expression, sqlBuilder, parameters);
                sqlBuilder.Append(")");
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeYear)
            {
                AppendDatePart(visitor, exp, sqlBuilder, parameters, "YEAR");
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeMonth)
            {
                AppendDatePart(visitor, exp, sqlBuilder, parameters, "MONTH");
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeDay)
            {
                AppendDatePart(visitor, exp, sqlBuilder, parameters, "DAY");
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeHour)
            {
                AppendDatePart(visitor, exp, sqlBuilder, parameters, "HOUR");
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeMinute)
            {
                AppendDatePart(visitor, exp, sqlBuilder, parameters, "HOUR");
                return;
            }
            if (member == ConstantDefined.PropertyDateTimeSecond)
            {
                AppendDatePart(visitor, exp, sqlBuilder, parameters, "SECOND");
                return;
            }
            /* MySql is not supports MILLISECOND */
            if (member == ConstantDefined.PropertyDateTimeDayOfWeek)
            {
                sqlBuilder.Append("(");
                AppendDatePart(visitor, exp, sqlBuilder, parameters, "DAYOFWEEK");
                sqlBuilder.Append(" - 1)");
            }
        }

        private void AppendDatePart(ExpressionVisitor visitor, MemberExpression exp, StringBuilder sqlBuilder, DynamicParameters parameters, string functionName)
        {
            sqlBuilder.Append(functionName);
            sqlBuilder.Append("(");
            visitor.InternalVisit(exp.Expression, sqlBuilder, parameters);
            sqlBuilder.Append(")");
        }

        /// <summary>
        /// 处理日期
        /// </summary>
        public void DateTimeAddMethod(ExpressionVisitor visitor, MethodCallExpression e, string function, StringBuilder sqlBuilder, DynamicParameters parameters)
        {
            sqlBuilder.Append("DATE_ADD(");
            visitor.InternalVisit(e.Object, sqlBuilder, parameters);
            sqlBuilder.Append(",INTERVAL ");
            visitor.InternalVisit(e.Arguments[0], sqlBuilder, parameters);
            sqlBuilder.AppendFormat(" {0} ", function);
            sqlBuilder.Append(")");
        }

        public bool HandleStringLength(MemberExpression memberExpression, ExpressionVisitor visitor, StringBuilder sqlBuilder, DynamicParameters parameters)
        {
            if (memberExpression.Member.Name != "Length")
            {
                return false;
            }
            sqlBuilder.Append("LENGTH(");
            visitor.InternalVisit(memberExpression.Expression, sqlBuilder, parameters);
            sqlBuilder.Append(")");
            return true;
        }
    }
}
