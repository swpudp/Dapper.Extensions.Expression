using System;

namespace Dapper.Extensions.Expression.WebTest.Model
{
    /// <summary>
    /// 业主
    /// </summary>
    public class Owner
    {
        public required string Id { get; set; }

        public required string RoomId { get; set; }

        public required string Code { get; set; }

        public required string Name { get; set; }

        public string Sex { get; set; }

        public int Age { get; set; }

        public string IdNo { get; set; }

        public required string Tel { get; set; }

        public string Hometown { get; set; }

        public string Workplace { get; set; }

        public string Remark { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        public int Version { get; set; }
    }
}
