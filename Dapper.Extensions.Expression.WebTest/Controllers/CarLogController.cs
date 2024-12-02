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

