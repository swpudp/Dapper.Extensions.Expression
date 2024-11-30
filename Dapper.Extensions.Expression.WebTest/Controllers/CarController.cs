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
    public class CarController : ControllerBase
    {
        [HttpPost("init")]
        public async Task<bool> Init()
        {
            using IDbConnection connection = CreateConnection();
            int index = 0;
            int pageSize = 4000;
            string sex = OwnerSex.Male.ToString().ToLower();
            while (true)
            {
                IList<Owner> owners = connection.Query<Owner>().Where(f => f.Sex == sex && f.Age >= 30 && f.Age <= 50).TakePage(index, pageSize).ToList<Owner>();
                if (!owners.Any())
                {
                    break;
                }
                index++;
                IList<Car> cars = Convert(owners).ToList();
                await connection.InsertBulkAsync(cars);
            }
            return true;
        }

        private static IEnumerable<Car> Convert(IList<Owner> owners)
        {
            foreach (var owner in owners)
            {
                Car car = new Car
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    OwnerId = owner.Id,
                    Code = $"川E{GetCarNo()}",
                    Color = GetColor(),
                    Type = GetCarType(),
                    CreateTime = DateTime.Now,
                    Version = 1
                };
                yield return car;
            }
        }

        private static List<CarType> carTypes = [CarType.Small, CarType.Large, CarType.Mid];
        private static List<string> colors = ["黑色", "白色", "红色", "灰色", "银色", "棕色", "紫色"];
        private static string numbers = "0123456789";
        private static string alphas = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        private static CarType GetCarType()
        {
            return carTypes[Random.Shared.Next(0, carTypes.Count)];
        }

        private static string GetColor()
        {
            return colors[Random.Shared.Next(0, colors.Count)];
        }

        private static string GetCarNo()
        {
            List<char> chars = [numbers[Random.Shared.Next(0, numbers.Length)], numbers[Random.Shared.Next(0, numbers.Length)], numbers[Random.Shared.Next(0, numbers.Length)], alphas[Random.Shared.Next(0, alphas.Length)], alphas[Random.Shared.Next(0, alphas.Length)]];
            return new string([.. chars.OrderBy(f => Random.Shared.Next())]);
        }

        private static IDbConnection CreateConnection()
        {
            IDbConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;database=big_data_tutorial;uid=root;pwd=Q1@we34r;charset=utf8");
            return connection;
        }
    }
}

