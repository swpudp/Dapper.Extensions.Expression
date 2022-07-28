using System;

namespace Dapper.Extensions.Expression
{
    /// <summary>
    /// 函数
    /// </summary>
    public static class Function
    {
        /// <summary>
        /// 计数
        /// </summary>
        /// <returns></returns>
        public static int Count()
        {
            return 0;
        }

        /// <summary>
        /// 最大值
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public static TResult Max<TResult>(TResult p)
        {
            return p;
        }

        /// <summary>
        /// 最小值
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public static TResult Min<TResult>(TResult p)
        {
            return p;
        }

        /// <summary>
        /// 求和
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static TResult Sum<TResult>(TResult p)
        {
            return p;
        }

        /// <summary>
        /// 平均值
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public static TResult Average<TResult>(TResult p)
        {
            return p;
        }

        public static int? DiffYears(DateTime? dateTime1, DateTime? dateTime2)
        {
            throw new NotSupportedException();
        }
        public static int? DiffMonths(DateTime? dateTime1, DateTime? dateTime2)
        {
            throw new NotSupportedException();
        }
        public static int? DiffDays(DateTime? dateTime1, DateTime? ddateTime2)
        {
            throw new NotSupportedException();
        }
        public static int? DiffHours(DateTime? dateTime1, DateTime? dateTime2)
        {
            throw new NotSupportedException();
        }
        public static int? DiffMinutes(DateTime? dateTime1, DateTime? dateTime2)
        {
            throw new NotSupportedException();
        }
        public static int? DiffSeconds(DateTime? dateTime1, DateTime? dateTime2)
        {
            throw new NotSupportedException();
        }
        public static int? DiffMilliseconds(DateTime? dateTime1, DateTime? dateTime2)
        {
            throw new NotSupportedException();
        }
        public static int? DiffMicroseconds(DateTime? dateTime1, DateTime? dateTime2)
        {
            throw new NotSupportedException();
        }
    }
}
