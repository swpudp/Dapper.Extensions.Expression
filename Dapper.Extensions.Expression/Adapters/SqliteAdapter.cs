using Dapper.Extensions.Expression.Utilities;
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
            sb.AppendFormat(" LIMIT {0},{1}", currentPage * pageSize, pageSize);
        }

        public void HandleDateTime(MemberExpression exp, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            MemberInfo member = exp.Member;
            if (member == ConstantDefined.PropertyDateTimeNow)
            {
                sqlBuilder.Append("date('now')");
            }
            else if (member == ConstantDefined.PropertyDateTimeUtcNow)
            {
                sqlBuilder.Append("date('now','utc')");
            }
            else if (member == ConstantDefined.PropertyDateTimeToday)
            {
                sqlBuilder.Append("strftime('%Y-%m-%d','now')");
            }
            else if (member == ConstantDefined.PropertyDateTimeDate)
            {
                AppendDatePart(exp, sqlBuilder, parameters, "strftime", "%Y-%m-%d", appendParameter);
            }
            else if (member == ConstantDefined.PropertyDateTimeYear)
            {
                AppendDatePart(exp, sqlBuilder, parameters, "strftime", "%Y", appendParameter);
            }
            else if (member == ConstantDefined.PropertyDateTimeMonth)
            {
                AppendDatePart(exp, sqlBuilder, parameters, "strftime", "%m", appendParameter);
            }
            else if (member == ConstantDefined.PropertyDateTimeDay)
            {
                AppendDatePart(exp, sqlBuilder, parameters, "strftime", "%d", appendParameter);
            }
            else if (member == ConstantDefined.PropertyDateTimeHour)
            {
                AppendDatePart(exp, sqlBuilder, parameters, "strftime", "%H", appendParameter);
            }
            else if (member == ConstantDefined.PropertyDateTimeMinute)
            {
                AppendDatePart(exp, sqlBuilder, parameters, "strftime", "%M", appendParameter);
            }
            else if (member == ConstantDefined.PropertyDateTimeSecond)
            {
                AppendDatePart(exp, sqlBuilder, parameters, "strftime", "%S", appendParameter);
            }
            else if (member == ConstantDefined.PropertyDateTimeDayOfWeek)
            {
                AppendDatePart(exp, sqlBuilder, parameters, "strftime", "%w", appendParameter);
            }
            else
            {
                throw new NotSupportedException("not support");
            }
        }

        private void AppendDatePart(MemberExpression exp, StringBuilder sqlBuilder, DynamicParameters parameters, string functionName, string fmt, bool appendParameter)
        {
            sqlBuilder.Append(functionName).Append("(\'").Append(fmt).Append("\',");
            WhereExpressionVisitor.InternalVisit(exp.Expression, this, sqlBuilder, parameters, appendParameter);
            sqlBuilder.Append(')');
        }

        /// <summary>
        /// 处理日期
        /// </summary>
        public void DateTimeAddMethod(MethodCallExpression e, string function, ISqlAdapter adapter, StringBuilder sqlBuilder, DynamicParameters parameters, bool appendParameter)
        {
            if (!(e.Object is MemberExpression me))
            {
                throw new NotSupportedException("不支持的表达式:" + e);
            }
            sqlBuilder.Append("datetime(");
            //不是属性访问
            if (me.Expression == null)
            {
                sqlBuilder.Append("'now','");
            }
            else
            {
                if (me.Expression is MemberExpression pme)
                {
                    if (pme.Expression is ConstantExpression)
                    {
                        object v = ExpressionEvaluator.Visit(me);
                        sqlBuilder.AppendFormat("'{0:yyyy-MM-dd HH:mm:ss}','", v);

                        //sqlBuilder.Append('\'');
                        //WhereExpressionVisitor.InternalVisit(ce, adapter, sqlBuilder, parameters, false);
                        //sqlBuilder.Append("','");
                    }
                    else
                    {
                        WhereExpressionVisitor.InternalVisit(pme, adapter, sqlBuilder, parameters, appendParameter);
                        sqlBuilder.Append(",'");
                    }
                }
                else
                {
                    WhereExpressionVisitor.InternalVisit(me, adapter, sqlBuilder, parameters, appendParameter);
                    sqlBuilder.Append(",'");
                }
            }
            sqlBuilder.Append(e.Arguments[0]).Append(' ').Append(function.ToLower()).Append('s').Append('\'').Append(')');
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
            builder.Append("CAST(COALESCE(");
            //参数部分
            action(e.Left, this, builder, appendParameter);
            builder.Append(',');
            //值部分
            action(e.Right, this, builder, appendParameter);
            builder.Append(") AS TEXT) AS ");
        }
    }
}
