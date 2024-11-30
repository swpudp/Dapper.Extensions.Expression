namespace Dapper.Extensions.Expression.WebTest.Model
{
    /// <summary>
    /// 小区
    /// </summary>
    public class Community
    {
        public required string Id { get; set; }

        public string DistrictId { get; set; }

        public string CityId { get; set; }

        /// <summary>
        /// 楼栋数量
        /// </summary>
        public int Houses { get; set; }

        /// <summary>
        /// 楼栋类型
        /// </summary>
        public HouseType HouseType { get; set; }

        /// <summary>
        /// 楼层数
        /// </summary>
        public int Floors { get; set; }

        /// <summary>
        /// 每楼住户数
        /// </summary>
        public int Rooms { get; set; }

        public required string Name { get; set; }

        public required string Location { get; set; }

        public string Remark { get; set; }

        public int Version { get; set; }
    }
}
