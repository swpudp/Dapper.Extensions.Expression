using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Dapper.Extensions.Expression
{
    /// <summary>
    /// 表达式常量定义
    /// </summary>
    internal static class ConstantDefined
    {
        /// <summary>
        /// 常量true表达式
        /// </summary>
        internal static readonly ConstantExpression BooleanTrue = System.Linq.Expressions.Expression.Constant(true);

        /// <summary>
        /// 常量false表达式
        /// </summary>
        internal static readonly ConstantExpression BooleanFalse = System.Linq.Expressions.Expression.Constant(false);

        /// <summary>
        /// 字符串Contains方法
        /// </summary>
        internal static readonly MethodInfo StringContains = typeof(string).GetMethod("Contains", new[] { typeof(string) });

        /// <summary>
        /// 字符串Equals方法
        /// </summary>
        internal static readonly MethodInfo StringEquals = typeof(string).GetMethod("Equals", new[] { typeof(string) });

        /// <summary>
        /// and类型节点
        /// </summary>
        internal static readonly ExpressionType[] AndAlsoNodeTypes = { ExpressionType.And, ExpressionType.AndAlso };

        /// <summary>
        /// or类型节点
        /// </summary>
        internal static readonly ExpressionType[] OrAlsoNodeTypes = { ExpressionType.Or, ExpressionType.OrElse };

        /// <summary>
        /// 当前时间方法定义
        /// </summary>
        internal static readonly PropertyInfo PropertyDateTimeNow = typeof(DateTime).GetProperty("Now");
        internal static readonly PropertyInfo PropertyDateTimeUtcNow = typeof(DateTime).GetProperty("UtcNow");
        internal static readonly PropertyInfo PropertyDateTimeToday = typeof(DateTime).GetProperty("Today");
        internal static readonly PropertyInfo PropertyDateTimeDate = typeof(DateTime).GetProperty("Date");


        internal static readonly PropertyInfo PropertyDateTimeYear = typeof(DateTime).GetProperty("Year");
        internal static readonly PropertyInfo PropertyDateTimeMonth = typeof(DateTime).GetProperty("Month");
        internal static readonly PropertyInfo PropertyDateTimeDay = typeof(DateTime).GetProperty("Day");
        internal static readonly PropertyInfo PropertyDateTimeHour = typeof(DateTime).GetProperty("Hour");
        internal static readonly PropertyInfo PropertyDateTimeMinute = typeof(DateTime).GetProperty("Minute");
        internal static readonly PropertyInfo PropertyDateTimeSecond = typeof(DateTime).GetProperty("Second");
        internal static readonly PropertyInfo PropertyDateTimeMillisecond = typeof(DateTime).GetProperty("Millisecond");
        internal static readonly PropertyInfo PropertyDateTimeDayOfWeek = typeof(DateTime).GetProperty("DayOfWeek");

        internal static readonly Type TypeOfDateTime = typeof(DateTime);
        internal static readonly Type TypeOfString = typeof(string);
        internal static readonly Type TypeOfBoolean = typeof(bool);
        internal static readonly Type TypeOfInt32 = typeof(int);

        internal const string MemberNameValue = "Value";
        internal const string MemberNameHasValue = "HasValue";
        internal const string GuidEmpty = "Empty";

        internal const string OrderAsc = "ASC";
        internal const string OrderDesc = "DESC";
    }
}
