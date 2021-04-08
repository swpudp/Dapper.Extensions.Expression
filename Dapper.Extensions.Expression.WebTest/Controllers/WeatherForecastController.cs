using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper.Extensions.Expression.Extensions;

namespace Dapper.Extensions.Expression.WebTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("QueryTest")]
        public IEnumerable<TestEntity> QueryTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = connection.Query<TestEntity>();
            query = query.Where(v => v.TestName.Contains("FD2D"));
            IList<TestEntity> data = query.ToList<TestEntity>();
            return data;
        }

        [HttpGet("TestEntity")]
        public async Task<bool> CreateTestEntity()
        {
            TestEntity testEntity = new TestEntity
            {
                Id = Guid.NewGuid(),
                Code = "123",
                Ignore = "1234",
                IsDelete = false,
                Items = new List<Item> { new Item { Id = Guid.NewGuid() } },
                Logged = DateTime.Now,
                Message = "test",
                TestName = nameof(WeatherForecastController),
                Number = "test",
                Type = TestType.Log
            };
            IDbConnection connection = CreateConnection();
            int result = await connection.InsertAsync(testEntity);
            return result > 0;
        }

        private static IDbConnection CreateConnection()
        {
            IDbConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8");
            return connection;
        }

    }

    [Table("testentity")]
    public class TestEntity : IEntity
    {
        [ExplicitKey]
        public Guid Id { get; set; }

        [Column("Name")]
        public string TestName { get; set; }

        public DateTime Logged { get; set; }

        public bool IsDelete { get; set; }

        public DateTime? UpdateTime { get; set; }

        public string Message { get; set; }

        public string Code { get; set; }

        public string Number { get; set; }

        [Computed]
        public TestType Type { get; set; }

        [NotMapped]
        public string Ignore { get; set; }

        [NotMapped]
        public IList<Item> Items { get; set; }
    }

    public interface IEntity
    {
        Guid Id { get; set; }
    }

    public enum TestType
    {
        Log,
        Trace
    }

    public class Item : IEntity
    {
        public Guid Id { get; set; }
    }
}
