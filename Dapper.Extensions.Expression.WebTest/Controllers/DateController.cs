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
    public class DateController : ControllerBase
    {
        [HttpPost("init")]
        public async Task<bool> Init([FromQuery] int year)
        {
            using IDbConnection connection = CreateConnection();
            IList<Date> dates = Convert(year).ToList();
            await connection.InsertBulkAsync(dates);
            return true;
        }

        private static IEnumerable<Date> Convert(int year)
        {
            DateTime d = new DateTime(year, 1, 1);
            while (d.Year == year)
            {
                Date date = new Date
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    DateId = d.ToString("yyyyMMdd"),
                    WeekId = (int)d.DayOfWeek,
                    WeekDay = (int)d.DayOfWeek,
                    Day = d.Day,
                    Month = d.Month,
                    Year = d.Year,
                    Quarter = 1,
                    IsWorkDay = (d.DayOfWeek != DayOfWeek.Sunday && d.DayOfWeek != DayOfWeek.Saturday) ? 1 : 0,
                    HolidayId = (int)d.DayOfWeek,
                    Version = 1
                };
                yield return date;
                d = d.AddDays(1);
            }
        }

        [HttpGet("{id}")]
        public async Task<Date> Get(string id)
        {
            using IDbConnection connection = CreateConnection();
            return await connection.Query<Date>().Where(f => f.Id == id).FirstOrDefaultAsync<Date>();
        }

        private static IDbConnection CreateConnection()
        {
            IDbConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;database=big_data_tutorial;uid=root;pwd=Q1@we34r;charset=utf8");
            return connection;
        }
    }
}
