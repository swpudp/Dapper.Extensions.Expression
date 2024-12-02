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
    public class RoomController : ControllerBase
    {
        [HttpPost("init")]
        public async Task<bool> Init([FromQuery] string cityId)
        {
            using IDbConnection connection = CreateConnection();
            IList<Community> communityList = connection.Query<Community>().Where(f => f.CityId == cityId).ToList<Community>();
            foreach (var community in communityList)
            {
                IList<House> houseList = connection.Query<House>().Where(f => f.CommunityId == community.Id).ToList<House>();
                IList<Room> rooms = Convert(community, houseList).ToList();
                await connection.InsertBulkAsync(rooms);
            }
            return true;
        }

        [HttpGet("{id}")]
        public async Task<Room> Get(string id)
        {
            using IDbConnection connection = CreateConnection();
            return await connection.Query<Room>().Where(f => f.Id == id).FirstOrDefaultAsync<Room>();
        }

        private static IEnumerable<Room> Convert(Community community, IList<House> houseList)
        {
            foreach (var house in houseList)
            {
                foreach (var floorIndex in Enumerable.Range(0, house.Floors))
                {
                    foreach (var roomIndex in Enumerable.Range(0, house.Rooms))
                    {
                        string roomSeq = (roomIndex + 1).ToString().PadLeft(2, '0');
                        Room room = new Room
                        {
                            Id = ObjectId.GenerateNewId().ToString(),
                            CommunityId = community.Id,
                            HouseId = house.Id,
                            Code = $"{floorIndex + 1}{roomSeq}",
                            Name = $"{house.Code}-{floorIndex + 1}{roomSeq}",
                            Populations = Random.Shared.Next(1, 5),
                            Version = 1
                        };
                        yield return room;
                    }
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

