using System;

namespace Dapper.Extensions.Expression.WebTest.Model
{
    /// <summary>
    /// 车辆进出日志
    /// </summary>
    public class CarLog
    {
        public required string Id { get; set; }

        public required string CommunityId { get; set; }

        public DateTime OpenTime { get; set; }

        public CarLogType Type { get; set; }

        public required string CarNo { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        public int Version { get; set; }
    }
}
