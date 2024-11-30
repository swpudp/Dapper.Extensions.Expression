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
        [HttpPost("init")]
        public async Task<bool> Init([FromBody] GuardLog guardLog)
        {
            DateTime openTime = DateTime.Now.AddDays(Random.Shared.Next(0, 30) * -1);
            using IDbConnection connection = CreateConnection();
            guardLog.Id = ObjectId.GenerateNewId().ToString();
            guardLog.OpenTime = openTime;
            guardLog.CreateTime = openTime;
            guardLog.Version = 1;
            guardLog.OpenMode = guardModes[Random.Shared.Next(0, guardModes.Length)];
            await connection.InsertAsync(guardLog);
            return true;
        }

        private static readonly GuardMode[] guardModes = [GuardMode.Card, GuardMode.Password, GuardMode.Face, GuardMode.Remote];

        private static IDbConnection CreateConnection()
        {
            IDbConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;database=big_data_tutorial;uid=root;pwd=Q1@we34r;charset=utf8");
            return connection;
        }
    }
}

