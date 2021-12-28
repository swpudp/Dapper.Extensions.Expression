using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper.Extensions.Expression.Extensions;
using System.Transactions;

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

        [HttpGet("TransactionAsync")]
        public async Task<int> TransactionAsync()
        {
            using (var trans = new TransactionScope())
            {
                var buyers = Enumerable.Range(0, 5000).Select(f => CreateBuyer()).ToList();
                IDbConnection connection = CreateConnection();
                int result = await connection.InsertBulkAsync(buyers);
                trans.Complete();
                return result;
            }
        }

        [HttpGet("Transaction")]
        public async Task<int> Transaction()
        {
            using (var trans = new TransactionScope())
            {
                var buyers = Enumerable.Range(0, 5000).Select(f => CreateBuyer()).ToList();
                IDbConnection connection = CreateConnection();
                int result = connection.InsertBulk(buyers);
                trans.Complete();
                return await Task.FromResult(result);
            }
        }

        private static Buyer CreateBuyer()
        {
            string id = Guid.NewGuid().ToString().Replace("-", string.Empty).ToUpper();
            Buyer buyer = new Buyer
            {
                Id = Guid.NewGuid(),
                Code = id.Substring(0, 4),
                CreateTime = DateTime.Now,
                Email = id.Substring(4, 8) + "@" + id.Substring(10, 2) + ".com",
                Identity = id.Substring(10, 10),
                IsActive = true,
                IsDelete = false,
                Mobile = "13900000000",
                Name = id.Substring(20, 4),
                Type = BuyerType.Company,
            };
            return buyer;
        }

        private static IDbConnection CreateConnection()
        {
            IDbConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;database=dapper_extension;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8");
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

    [Table("buyer")]
    public class Buyer : IEntity
    {
        [ExplicitKey] public Guid Id { get; set; }

        public string Name { get; set; }

        public BuyerType Type { get; set; }

        public string Code { get; set; }

        public string Identity { get; set; }

        public string Email { get; set; }

        public string Mobile { get; set; }

        public bool IsDelete { get; set; }

        public bool? IsActive { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }
    }


    public enum BuyerType
    {
        Person,
        Company,
        Other
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
