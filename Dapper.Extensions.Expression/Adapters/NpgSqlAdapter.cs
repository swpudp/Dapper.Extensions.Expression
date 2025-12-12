using Dapper.Extensions.Expression.Utilities;
using Dapper.Extensions.Expression.Visitors;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Dapper.Extensions.Expression.Adapters
{
    /// <summary>
    /// The PostgreSQL database adapter.
    /// </summary>
    internal class NpgSqlAdapter : AbstractSqlAdapter, ISqlAdapter
    {
        public override int MaxParameterCount => 4000;

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
            int currentPage = page > 1 ? page - 1 : 0;
            sb.AppendFormat(" LIMIT {0} OFFSET {1}", pageSize, currentPage * pageSize);
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
                sqlBuilder.Append(')');
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
            if (member == ConstantDefined.PropertyDateTimeDayOfWeek)
            {
                sqlBuilder.Append('(');
                AppendDatePart(exp, sqlBuilder, parameters, "DAYOFWEEK", appendParameter);
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
        public void DateTimeAddMethod(MethodCallExpression e, string function, ISqlAdapter adapter, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            sqlBuilder.Append("DATE_ADD(");
            WhereExpressionVisitor.InternalVisit(e.Object, this, sqlBuilder, parameters, appendParameter);
            sqlBuilder.Append(",INTERVAL ");
            WhereExpressionVisitor.InternalVisit(e.Arguments[0], this, sqlBuilder, parameters, appendParameter);
            sqlBuilder.AppendFormat(" {0} ", function);
            sqlBuilder.Append(')');
        }

        public bool HandleStringLength(MemberExpression memberExpression, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            if (memberExpression.Member.Name != "Length")
            {
                return false;
            }
            sqlBuilder.Append("LENGTH(");
            WhereExpressionVisitor.InternalVisit(memberExpression.Expression, this, sqlBuilder, parameters, appendParameter);
            sqlBuilder.Append(')');
            return true;
        }


        public override string ParseBool(bool v)
        {
            return v ? "true" : "false";
        }

        public void VisitCoalesce(BinaryExpression e, StringBuilder builder, Action<System.Linq.Expressions.Expression, ISqlAdapter, StringBuilder> action)
        {
            builder.Append("CAST(COALESCE(");
            action(e.Left, this, builder);
            builder.Append(',');
            action(e.Right, this, builder);
            builder.Append(") AS CHAR) AS ");
        }

        public override void AddParameter(StringBuilder builder, PropertyInfo property)
        {
            if (property.IsDefined(typeof(JsonbAttribute)))
            {
                AddParameter(builder, property.Name+"::jsonb");
            }
            else
            {
                base.AddParameter(builder, property);
            }
        }

        public override void AppendBinaryColumn(StringBuilder sb, MemberInfo memberInfo, out string name)
        {
            string ext = memberInfo.IsDefined(typeof(JsonbAttribute)) ? "::jsonb" : string.Empty;
            ColumnAttribute columnAttribute = memberInfo.GetCustomAttribute<ColumnAttribute>();
            if (columnAttribute == null)
            {
                name = memberInfo.Name;
                sb.AppendFormat("{0}{1}{2} = {3}{4}", LeftQuote, NamingUtils.GetName(memberInfo.Name), RightQuote, ParameterPrefix, memberInfo.Name+ext);
            }
            else
            {
                name = columnAttribute.Name;
                sb.AppendFormat("{0}{1}{2} = {3}{4}", LeftQuote, NamingUtils.GetName(columnAttribute.Name), RightQuote, ParameterPrefix, columnAttribute.Name + ext);
            }
        }

        public override void AddParameter(StringBuilder builder, PropertyInfo property, int index)
        {
            if (property.IsDefined(typeof(JsonbAttribute)))
            {
                builder.AppendFormat("{0}{1}{2}::jsonb", ParameterPrefix, property.Name, index);
            }
            else
            {
                base.AddParameter(builder, property, index);
            }
        }

        public override void AddParameter(Dictionary<string, object> parameters, PropertyInfo property, int index, object value)
        {
            if (property.IsDefined(typeof(JsonbAttribute)))
            {
                parameters.Add(string.Format("{0}{1}{2}::jsonb", ParameterPrefix, property.Name, index), value);
            }
            else
            {
                base.AddParameter(parameters, property, index, value);
            }
        }
    }
}
