namespace Dapper.Extensions.Expression.WebTest.Model
{
    /// <summary>
    /// 楼栋规划
    /// </summary>
    public class HousePlanning
    {
        public required string Id { get; set; }

        public required string DistrictId { get; set; }

        public required string CityId { get; set; }

        public required DistrictLevel Level { get; set; }

        public float Ratio { get; set; }

        public int Populations { get; set; }

        public HouseType HouseType { get; set; }

        public int Floors { get; set; }

        public int Rooms { get; set; }

        public int Houses { get; set; }
    }
}
