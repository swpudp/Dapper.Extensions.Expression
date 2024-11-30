namespace Dapper.Extensions.Expression.WebTest.Model
{
    /// <summary>
    /// 日期
    /// </summary>
    public class Date
    {
        public required string Id { get; set; }

        public string DateId { get; set; }

        public int WeekId { get; set; }

        public int WeekDay { get; set; }

        public int Day { get; set; }

        public int Month { get; set; }

        public int Quarter { get; set; }

        public int Year { get; set; }

        public int IsWorkDay { get; set; }

        public int HolidayId { get; set; }

        public int Version { get; set; }
    }
}
