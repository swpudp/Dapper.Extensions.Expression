using Dapper.Extensions.Expression.WebTest.Model;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Extensions.Expression.WebTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HouseController : ControllerBase
    {
        [HttpPost("init")]
        public async Task<bool> Init([FromQuery] string cityId)
        {
            using IDbConnection connection = CreateConnection();
            IList<Community> communityList = connection.Query<Community>().Where(f => f.CityId == cityId).ToList<Community>();
            IList<District> districtList = connection.Query<District>().Where(f => f.ParentId == cityId).ToList<District>();
            IList<House> districts = Convert(communityList, districtList).ToList();
            await connection.InsertBulkAsync(districts);
            return true;
        }

        [HttpPost("Planning")]
        public async Task<bool> Planning([FromQuery] string cityCode)
        {
            using IDbConnection connection = CreateConnection();
            IList<District> cityList = connection.Query<District>().Where(f => f.Code.StartsWith(cityCode)).ToList<District>();
            District city = cityList.First(f => f.Code == cityCode);
            IList<HousePlanning> townHousePlannings = CreateHousePlannings(cityList, city).ToList();
            await connection.InsertBulkAsync(townHousePlannings);
            IList<HousePlanning> cityHousePlannings = CreateSummaryHousePlannings(cityList, city, townHousePlannings).ToList();
            await connection.InsertBulkAsync(cityHousePlannings);
            return true;
        }

        [HttpGet("{id}")]
        public async Task<House> Get(string id)
        {
            using IDbConnection connection = CreateConnection();
            return await connection.Query<House>().Where(f => f.Id == id).FirstOrDefaultAsync<House>();
        }

        [HttpGet("Planning/{id}")]
        public async Task<HousePlanning> GetHousePlanning(string id)
        {
            using IDbConnection connection = CreateConnection();
            return await connection.Query<HousePlanning>().Where(f => f.Id == id).FirstOrDefaultAsync<HousePlanning>();
        }

        /// <summary>
        /// 房屋配置
        /// 房屋类型,最小人数比例,最大人数比例，楼层数，最小房间数，最大房间数
        /// </summary>
        private static readonly List<Tuple<HouseType, float, float, int, int, int>> ratioRangeConfs = [new(HouseType.LowRise, 0.1f, 0.2f, 6, 2, 2), new(HouseType.LowRise, 0.2f, 0.3f, 11, 2, 4), new(HouseType.LowRise, 0.3f, 0.4f, 18, 4, 6), new(HouseType.LowRise, 0.3f, 0.4f, 26, 4, 6)];

        private static IEnumerable<HousePlanning> CreateHousePlannings(IList<District> cityList, District city)
        {
            foreach (var town in cityList.Where(f => f.Level == DistrictLevel.Town))
            {
                int totalPopulations = 0;
                foreach (Tuple<HouseType, float, float, int, int, int> ratioRangeConf in ratioRangeConfs)
                {
                    int index = ratioRangeConfs.IndexOf(ratioRangeConf);
                    float ratio = (float)Math.Round(Random.Shared.NextSingle() * (ratioRangeConf.Item3 - ratioRangeConf.Item2) + ratioRangeConf.Item2, 1);
                    int housePopulations = index >= ratioRangeConfs.Count - 1 ? town.Populations - totalPopulations : System.Convert.ToInt32(town.Populations * ratio);
                    totalPopulations += housePopulations;
                    //每层房间数范围
                    int[] perFloorRoomsRange = [ratioRangeConf.Item5, ratioRangeConf.Item6];
                    int perFloorRooms = perFloorRoomsRange[Random.Shared.Next(0, perFloorRoomsRange.Length)];
                    //按照平均每间房3人规划
                    int totalHouses = housePopulations / (perFloorRooms * ratioRangeConf.Item4 * 3);
                    HousePlanning housePlanning = new HousePlanning
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        CityId = city.Id,
                        DistrictId = town.Id,
                        Level = town.Level,
                        Ratio = housePopulations * 1f / town.Populations,
                        Populations = housePopulations,
                        HouseType = ratioRangeConf.Item1,
                        Floors = ratioRangeConf.Item4,
                        Rooms = perFloorRooms,
                        Houses = totalHouses
                    };
                    yield return housePlanning;
                }
            }
        }

        private static IEnumerable<HousePlanning> CreateSummaryHousePlannings(IList<District> cityList, District city, IList<HousePlanning> housePlannings)
        {
            HousePlanning housePlanning = new HousePlanning
            {
                Id = ObjectId.GenerateNewId().ToString(),
                CityId = city.Id,
                DistrictId = city.Id,
                Level = city.Level,
                Ratio = 1f,
                Populations = housePlannings.Sum(f => f.Populations),
                HouseType = HouseType.All,
                Floors = 0,
                Rooms = 0,
                Houses = housePlannings.Sum(f => f.Houses)
            };
            yield return housePlanning;
        }

        private static IEnumerable<House> Convert(IList<Community> communityList, IList<District> districtList)
        {
            foreach (Community community in communityList)
            {
                District town = districtList.First(f => f.Id == community.DistrictId);
                foreach (var hourseIndex in Enumerable.Range(0, community.Houses))
                {
                    float area = community.Houses * 666f * (Random.Shared.NextSingle() * 0.7f + 0.8f);
                    DateTime start = DateTime.Now.AddYears(-1 * Random.Shared.Next(1, 20));
                    House house = new House
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        CommunityId = community.Id,
                        Code = $"{hourseIndex + 1}",
                        Name = $"{hourseIndex + 1}#栋",
                        Floors = community.Floors,
                        Rooms = community.Rooms,
                        BuildStart = start,
                        BuildEnd = start.AddYears(1),
                        Area = area - area % 100,
                        Version = 1
                    };
                    yield return house;
                }
            }
        }

        private static IDbConnection CreateConnection()
        {
            IDbConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;database=big_data_tutorial;uid=root;pwd=Q1@we34r;charset=utf8");
            return connection;
        }
    }
}

