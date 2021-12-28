using BenchmarkDotNet.Attributes;
using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace Dapper.Extensions.Expression.BenchmarkTest
{
    public class GenerateCommandText
    {
        [Benchmark]
        public string CreateWhereIfEmptyParamSql()
        {
            QueryParam queryParam = new QueryParam();
            IDbConnection connection = new MySqlConnection();
            IQuery query = connection.Query<Order>()
                  .WhereIf(queryParam.CreateTime.HasValue, f => f.CreateTime > queryParam.CreateTime)
                  .WhereIf(queryParam.IsDelete == true, f => f.IsDelete)
                  .WhereIf(!string.IsNullOrWhiteSpace(queryParam.Key), f => f.Remark.Contains(queryParam.Key))
                  .Select(g => g.Id);
            return query.GetCommandText();
        }


        /// <summary>
        /// whereif测试
        /// </summary>
        [Benchmark]
        public string CreateWhereIfFullParamSql()
        {
            QueryParam queryParam = new QueryParam { CreateTime = DateTime.Now.AddDays(-10), IsDelete = true, Key = "1234" };
            IDbConnection connection = new MySqlConnection();
            IQuery query = connection.Query<Order>()
                  .WhereIf(queryParam.CreateTime.HasValue, f => f.CreateTime > queryParam.CreateTime)
                  .WhereIf(queryParam.IsDelete == true, f => f.IsDelete)
                  .WhereIf(!string.IsNullOrWhiteSpace(queryParam.Key), f => f.Remark.Contains(queryParam.Key))
                  .Select(g => g.Id);
            return query.GetCommandText();
        }

        /// <summary>
        /// whereif测试
        /// </summary>
        [Benchmark]
        public string CreateWhereIfPartialParamSql()
        {
            QueryParam queryParam = new QueryParam { CreateTime = DateTime.Now.AddDays(-10), IsDelete = true };
            IDbConnection connection = new MySqlConnection();
            IQuery query = connection.Query<Order>()
                  .WhereIf(queryParam.CreateTime.HasValue, f => f.CreateTime > queryParam.CreateTime)
                  .WhereIf(queryParam.IsDelete == true, f => f.IsDelete)
                  .WhereIf(!string.IsNullOrWhiteSpace(queryParam.Key), f => f.Remark.Contains(queryParam.Key))
                  .Select(g => g.Id);
            return query.GetCommandText();
        }
    }
}
