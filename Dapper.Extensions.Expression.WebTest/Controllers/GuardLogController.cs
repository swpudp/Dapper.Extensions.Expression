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
    public class GuardLogController(IConnectionMultiplexer connectionMultiplexer) : ControllerBase
    {
        [HttpPost("open")]
        public async Task<bool> OpenGuard([FromBody] AddGuardLogReq addGuardLogReq)
        {
            DateTime openTime = DateTime.Now.AddDays(Random.Shared.Next(0, 30) * -1);
            using IDbConnection connection = CreateConnection();
            GuardLog guardLog = new GuardLog
            {
                Id = ObjectId.GenerateNewId().ToString(),
                OwnerId = addGuardLogReq.OwnerId,
                OpenTime = openTime,
                CreateTime = openTime,
                Version = 1,
                OpenMode = addGuardLogReq.OpenMode
            };
            await connection.InsertAsync(guardLog);
            return true;
        }

        [HttpGet("open-rd")]
        public async Task<bool> OpenGuardRandom()
        {
            IDatabase database = connectionMultiplexer.GetDatabase();
            long listLength = await database.ListLengthAsync("view_owner");
            if (listLength == 0)
            {
                return false;
            }
            RedisValue redisValue = await database.ListGetByIndexAsync("view_owner", Random.Shared.NextInt64(0, listLength));
            if (!redisValue.HasValue)
            {
                return false;
            }
            GuardLog guardLog = new GuardLog
            {
                Id = ObjectId.GenerateNewId().ToString(),
                OwnerId = redisValue.ToString(),
                OpenTime = DateTime.Now,
                CreateTime = DateTime.Now,
                Version = 1,
                OpenMode = guardModes[Random.Shared.Next(0, guardModes.Length)]
            };
            using IDbConnection connection = CreateConnection();
            await connection.InsertAsync(guardLog);
            return true;
        }

        private static readonly GuardMode[] guardModes = [GuardMode.Card, GuardMode.Password, GuardMode.Face, GuardMode.Remote];

        [HttpGet("{id}")]
        public async Task<GuardLog> Get(string id)
        {
            using IDbConnection connection = CreateConnection();
            return await connection.Query<GuardLog>().Where(f => f.Id == id).FirstOrDefaultAsync<GuardLog>();
        }

        private static IDbConnection CreateConnection()
        {
            IDbConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;database=big_data_tutorial;uid=root;pwd=Q1@we34r;charset=utf8");
            return connection;
        }
    }
}

