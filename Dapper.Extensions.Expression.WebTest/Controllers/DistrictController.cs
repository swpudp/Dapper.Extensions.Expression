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
    public class DistrictController : ControllerBase
    {
        [HttpPost("Import")]
        public async Task<bool> Import([FromBody] List<ImportVo> data)
        {
            using IDbConnection connection = CreateConnection();
            IList<District> districts = Convert(data).ToList();
            await connection.InsertBulkAsync(districts);
            return true;
        }

        private static IEnumerable<District> Convert(List<ImportVo> data)
        {
            foreach (ImportVo importVo in data.Where(f => f.Lv == DistrictLevel.Province))
            {
                District province = new District
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Name = importVo.Name,
                    Code = importVo.Code,
                    Level = DistrictLevel.Province,
                    Status = 0,
                    CreateTime = DateTime.Now
                };
                yield return province;
                foreach (var cityVo in data.Where(f => f.Parent == importVo.Code))
                {
                    District city = new District
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        ParentId = province.Id,
                        Name = cityVo.Name,
                        Code = cityVo.Code,
                        Level = DistrictLevel.City,
                        Status = 0,
                        CreateTime = DateTime.Now
                    };
                    yield return city;
                    foreach (var countyVo in data.Where(f => f.Parent == cityVo.Code))
                    {
                        District county = new District
                        {
                            Id = ObjectId.GenerateNewId().ToString(),
                            ParentId = city.Id,
                            Name = countyVo.Name,
                            Code = countyVo.Code,
                            Level = DistrictLevel.County,
                            Status = 0,
                            CreateTime = DateTime.Now
                        };
                        yield return county;
                        foreach (var townVo in data.Where(f => f.Parent == countyVo.Code))
                        {
                            District town = new District
                            {
                                Id = ObjectId.GenerateNewId().ToString(),
                                ParentId = county.Id,
                                Name = townVo.Name,
                                Code = townVo.Code,
                                Level = DistrictLevel.Town,
                                Status = 0,
                                CreateTime = DateTime.Now
                            };
                            yield return town;
                        }
                    }
                }
            }
        }

        private static IDbConnection CreateConnection()
        {
            IDbConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;database=big_data_tutorial;uid=root;pwd=Q1@we34r;charset=utf8");
            return connection;
        }
    }
}
