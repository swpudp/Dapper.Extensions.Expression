using Dapper.Extensions.Expression.WebTest.Model;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using StackExchange.Redis;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Extensions.Expression.WebTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CarLogController : ControllerBase
    {
        [HttpPost("drive-in")]
        public async Task<bool> DriveIn([FromBody] AddCarLogReq carLog)
        {
            CarLog driveIn = new CarLog
            {
                Id = ObjectId.GenerateNewId().ToString(),
                CommunityId = carLog.CommunityId,
                CarNo = carLog.CarNo,
                Type = CarLogType.DriveIn,
                OpenTime = carLog.OpenTime,
                CreateTime = DateTime.Now,
                Version = 1
            };
            using IDbConnection connection = CreateConnection();
            await connection.InsertAsync(driveIn);
            return true;
        }

        [HttpPost("drive-out")]
        public async Task<bool> DriveOut([FromBody] AddCarLogReq carLog)
        {
            CarLog driveOut = new CarLog
            {
                Id = ObjectId.GenerateNewId().ToString(),
                CommunityId = carLog.CommunityId,
                CarNo = carLog.CarNo,
                Type = CarLogType.DriveOut,
                OpenTime = carLog.OpenTime,
                CreateTime = DateTime.Now,
                Version = 1
            };
            using IDbConnection connection = CreateConnection();
            await connection.InsertAsync(driveOut);
            return true;
        }

        [HttpGet("DriveIn")]
        public async Task<ViewCar> DriveIn()
        {
            using IDbConnection connection = CreateConnection();
            ViewCar viewCar = await connection.Query<ViewCar>().NotExist<CarLog>((c, v) => c.CarNo == v.CarNo && v.Type == CarLogType.DriveIn && v.CreateTime >= DateTime.Today).FirstOrDefaultAsync<ViewCar>();
            if (viewCar == null)
            {
                return null;
            }
            CarLog driveIn = new CarLog
            {
                Id = ObjectId.GenerateNewId().ToString(),
                CommunityId = viewCar.CommunityId,
                CarNo = viewCar.CarNo,
                Type = CarLogType.DriveIn,
                OpenTime = DateTime.Now,
                CreateTime = DateTime.Now,
                Version = 1
            };
            await connection.InsertAsync(driveIn);
            return viewCar;
        }

        [HttpGet("DriveRd")]
        public async Task<ViewCar> DriveRd()
        {
            using IDbConnection connection = CreateConnection();
            ConnectionMultiplexer connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync("localhost");
            IDatabase database = connectionMultiplexer.GetDatabase();
            RedisValue redisValue = await database.ListRightPopAsync("view_car");
            if (!redisValue.HasValue)
            {
                return null;
            }
            string[] values = redisValue.ToString().Split("|");
            CarLog driveIn = new CarLog
            {
                Id = ObjectId.GenerateNewId().ToString(),
                CommunityId = values[0],
                CarNo = values[1],
                Type = carLogTypes[Random.Shared.Next(0, carLogTypes.Length)],
                OpenTime = DateTime.Now,
                CreateTime = DateTime.Now,
                Version = 1
            };
            await connection.InsertAsync(driveIn);
            return new ViewCar { CarNo=values[1], CommunityId=values[0] };
        }

        private static readonly CarLogType[] carLogTypes = { CarLogType.DriveIn, CarLogType.DriveOut };

        [HttpGet("DriveOut")]
        public async Task<ViewCar> DriveOut()
        {
            using IDbConnection connection = CreateConnection();
            ViewCar viewCar = await connection.Query<ViewCar>().NotExist<CarLog>((c, v) => c.CarNo == v.CarNo && v.Type == CarLogType.DriveOut && v.CreateTime >= DateTime.Today).FirstOrDefaultAsync<ViewCar>();
            if (viewCar == null)
            {
                return null;
            }
            CarLog driveIn = new CarLog
            {
                Id = ObjectId.GenerateNewId().ToString(),
                CommunityId = viewCar.CommunityId,
                CarNo = viewCar.CarNo,
                Type = CarLogType.DriveOut,
                OpenTime = DateTime.Now,
                CreateTime = DateTime.Now,
                Version = 1
            };
            await connection.InsertAsync(driveIn);
            return viewCar;
        }

        [HttpGet("{id}")]
        public async Task<CarLog> Get(string id)
        {
            using IDbConnection connection = CreateConnection();
            return await connection.Query<CarLog>().Where(f => f.Id == id).FirstOrDefaultAsync<CarLog>();
        }

        private static IDbConnection CreateConnection()
        {
            IDbConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;database=big_data_tutorial;uid=root;pwd=Q1@we34r;charset=utf8");
            return connection;
        }
    }
}

