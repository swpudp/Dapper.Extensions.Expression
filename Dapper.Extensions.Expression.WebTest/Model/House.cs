using System;

namespace Dapper.Extensions.Expression.WebTest.Model
{
    /// <summary>
    /// 楼栋
    /// </summary>
    public class House
    {
        public required string Id { get; set; }

        public string CommunityId { get; set; }

        public required string Code { get; set; }

        public required string Name { get; set; }

        public DateTime BuildStart { get; set; }

        public DateTime BuildEnd { get; set; }

        public int Floors { get; set; }

        public int Rooms { get; set; }

        public float Area { get; set; }

        public string Remark { get; set; }

        public int Version { get; set; }
    }
}
