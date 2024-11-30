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
    public class CarLogController : ControllerBase
    {
        [HttpPost("init")]
        public async Task<bool> Init([FromBody] CarLog carLog)
        {
            using IDbConnection connection = CreateConnection();
            DateTime start = carLog.OpenTime;
            DateTime end = start.AddHours(Random.Shared.Next(4, 10));
            CarLog driveIn = new CarLog
            {
                Id = ObjectId.GenerateNewId().ToString(),
                CommunityId = carLog.CommunityId,
                CarNo = carLog.CarNo,
                Type = CarLogType.DriveIn,
                OpenTime = start,
                CreateTime = DateTime.Now,
                Version = 1
            };
            CarLog driveOut = new CarLog
            {
                Id = ObjectId.GenerateNewId().ToString(),
                CommunityId = carLog.CommunityId,
                CarNo = carLog.CarNo,
                Type = CarLogType.DriveOut,
                OpenTime = end,
                CreateTime = DateTime.Now,
                Version = 1
            };
            await connection.InsertBulkAsync([driveIn, driveOut]);
            return true;
        }

        private static IDbConnection CreateConnection()
        {
            IDbConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;database=big_data_tutorial;uid=root;pwd=Q1@we34r;charset=utf8");
            return connection;
        }
    }
}

