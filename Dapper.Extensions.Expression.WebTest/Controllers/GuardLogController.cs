using Dapper.Extensions.Expression.WebTest.Model;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Extensions.Expression.WebTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GuardLogController : ControllerBase
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

        //private static readonly GuardMode[] guardModes = [GuardMode.Card, GuardMode.Password, GuardMode.Face, GuardMode.Remote];

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

