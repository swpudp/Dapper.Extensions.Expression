using System;

namespace Dapper.Extensions.Expression.WebTest.Model
{
    /// <summary>
    /// 行政区划
    /// </summary>
    public class District
    {
        public required string Id { get; set; }

        public string ParentId { get; set; }

        public DistrictLevel Level { get; set; }

        public required string Code { get; set; }

        public required string Name { get; set; }

        public int Status { get; set; }

        public int Populations { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        public string Remark { get; set; }

        public int Version { get; set; }
    }

    public class ImportVo
    {
        public required string Code { get; set; }

        public required string Name { get; set; }

        public string Parent { get; set; }

        public DistrictLevel Lv { get; set; }
    }
}
