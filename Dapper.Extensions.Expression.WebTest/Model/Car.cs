using System;

namespace Dapper.Extensions.Expression.WebTest.Model
{
    /// <summary>
    /// 车辆
    /// </summary>
    public class Car
    {
        public required string Id { get; set; }

        public required string OwnerId { get; set; }

        public required string Code { get; set; }

        public required string Color { get; set; }

        public CarType Type { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        public string Remark { get; set; }

        public int Version { get; set; }
    }

    public class ViewCar
    {
        public required string CommunityId { get; set; }

        public required string CarNo { get; set; }
    }
}
