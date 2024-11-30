namespace Dapper.Extensions.Expression.WebTest.Model
{
    /// <summary>
    /// 房间
    /// </summary>
    public class Room
    {
        public required string Id { get; set; }

        public required string CommunityId { get; set; }

        public required string HouseId { get; set; }

        public required string Code { get; set; }

        public required string Name { get; set; }

        public int Populations { get; set; }

        public string Remark { get; set; }

        public int Version { get; set; }
    }
}
