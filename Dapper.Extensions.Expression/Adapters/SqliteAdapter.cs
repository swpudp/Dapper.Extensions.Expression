using Dapper.Extensions.Expression.Visitors;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Dapper.Extensions.Expression.Adapters
{
    /// <summary>
    /// The SQLite database adapter.
    /// </summary>
    internal class SqliteAdapter : AbstractSqlAdapter, ISqlAdapter
    {
        public override int MaxParameterCount => 2000;

        public override string ParameterPrefix => "@";

        public override string LeftQuote => "\"";

        public override string RightQuote => "\"";

        /// <summary>
        /// 增加分页信息
        /// </summary>
        public void AppendPage(StringBuilder sb, int page, int pageSize)
        {
            if (page == 0 && pageSize == 0)
            {
                return;
            }
            int currentPage = Math.Max(page - 1, 0);
            sb.AppendFormat(" LIMIT {0} OFFSET {1}", currentPage * pageSize, pageSize);
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
