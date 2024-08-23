using BenchmarkDotNet.Attributes;
using Dapper.Extensions.Expression.Queries;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

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

        /// <summary>
        /// 实例化集合Contains测试
        /// </summary>
        [Benchmark]
        public string QueryNewListContains()
        {
            using IDbConnection connection = new MySqlConnection();
            Query<Order> query = connection.Query<Order>();
            query.Where(v => new List<string> { "A82639", "8064C0A3" }.Contains(v.SerialNo));
            return query.GetCommandText();
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [Benchmark]
        public string QueryListContains()
        {
            using IDbConnection connection = new MySqlConnection();
            Query<Order> query = connection.Query<Order>();
            IList<string> values = new List<string> { "A82639", "8064C0A3" };
            query.Where(v => values.Contains(v.SerialNo));
            return query.GetCommandText();
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [Benchmark]
        public string QueryArrayContains()
        {
            using IDbConnection connection = new MySqlConnection();
            string[] values = { "ABC", "BCD" };
            Query<Order> query = connection.Query<Order>().Where(v => values.Contains(v.Remark));
            return query.GetCommandText();
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [Benchmark]
        public string QueryIEnumerableContains()
        {
            using IDbConnection connection = new MySqlConnection();
            IEnumerable<string> values = new[] { "EFC", "C0A3" };
            Query<Order> query = connection.Query<Order>().Where(v => values.Contains(v.SerialNo));
            return query.GetCommandText();
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [Benchmark]
        public string QueryBoolNot()
        {
            using IDbConnection connection = new MySqlConnection();
            Query<Order> query = connection.Query<Order>();
            query.Where(v => !v.IsDelete);
            return query.GetCommandText();
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [Benchmark]
        public string QueryBoolAccess()
        {
            using IDbConnection connection = new MySqlConnection();
            Query<Order> query = connection.Query<Order>();
            query.Where(v => v.IsDelete);
            return query.GetCommandText();
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [Benchmark]
        public string QueryParamQuery()
        {
            using IDbConnection connection = new MySqlConnection();
            Query<Order> query = connection.Query<Order>();
            QueryParam queryParam = new QueryParam
            {
                IsDelete = false,
                CreateTime = DateTime.Now.AddDays(-1)
            };
            query.Where(v => v.IsDelete == queryParam.IsDelete && v.CreateTime > queryParam.CreateTime);
            return query.GetCommandText();
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [Benchmark]
        public string QueryMultipleOr()
        {
            using IDbConnection connection = new MySqlConnection();
            Query<Order> query = connection.Query<Order>();
            QueryParam queryParam = new QueryParam
            {
                IsDelete = false,
                CreateTime = new DateTime(2021, 3, 20),
                Key = "DB"
            };
            query.Where(v => !v.IsDelete && (v.Remark.Contains(queryParam.Key) || v.CreateTime > queryParam.CreateTime));
            return query.GetCommandText();
        }

        /// <summary>
        /// 条件块测试
        /// </summary>
        [Benchmark]
        public string QueryConditionBlock()
        {
            using IDbConnection connection = new MySqlConnection();
            Expression<Func<Order, bool>> where = v => v.Status == Status.Draft && (v.IsDelete == false || v.Remark.Contains("FD")) && (v.SerialNo.Contains("FD") || v.SerialNo == "GD") && (v.CreateTime > new DateTime(2021, 3, 12) || v.UpdateTime < DateTime.Now.Date);
            Query<Order> query = connection.Query<Order>();
            query.Where(where);
            return query.GetCommandText();
        }

        /// <summary>
        /// 排序
        /// </summary>
        [Benchmark]
        public string QueryOrderAndPaging()
        {
            using IDbConnection connection = new MySqlConnection();
            Query<Order> query = connection.Query<Order>();
            query.TakePage(1, 10).OrderByDescending(f => f.CreateTime);
            return query.GetCommandText();
        }

        [Benchmark]
        public string SelectObject()
        {
            using IDbConnection connection = new MySqlConnection();
            Query<Order> query = connection.Query<Order>().Where(f => f.Remark.Contains("ABCD")).Select(f => new
            {
                f.Id,
                A = f.BuyerId,
                B = f.SerialNo,
                C = f.Remark,
                D = f.Status,
                E = f.SignState,
                F = f.Amount,
                G = f.Freight,
                H = f.DocId,
                I = f.IsDelete,
                J = f.IsActive,
                K = f.CreateTime,
                L = f.UpdateTime,
                M = LogType.Trace
            });
            return query.GetCommandText();
        }

        [Benchmark]
        public string SelectParameterObject()
        {
            using IDbConnection connection = new MySqlConnection();
            Query<Order> query = connection.Query<Order>();
            query.Where(f => f.Remark.Contains("ABCD")).Select(f => new { f });
            return query.GetCommandText();
        }

        [Benchmark]
        public string SelectAliasEntity()
        {
            using IDbConnection connection = new MySqlConnection();
            Query<Order> query = connection.Query<Order>().Where(f => f.Remark.Contains("ABCD")).Select(f => new OrderAliasModel
            {
                Id = f.Id,
                A = f.BuyerId,
                B = f.SerialNo,
                C = f.Remark,
                D = f.Status,
                E = f.SignState,
                F = f.Amount,
                G = f.Freight,
                H = f.DocId,
                I = f.IsDelete,
                J = f.IsActive,
                K = f.CreateTime,
                L = f.UpdateTime,
                M = LogType.Trace
            });
            return query.GetCommandText();
        }

        [Benchmark]
        public string SelectEntitySwapProperty()
        {
            using IDbConnection connection = new MySqlConnection();
            Query<Order> query = connection.Query<Order>().Where(f => f.Remark.Contains("ABCD")).Select(f => new Order
            {
                Id = f.Id,
                BuyerId = f.BuyerId,
                SerialNo = f.Remark,
                Remark = f.SerialNo,
                Status = f.Status,
                SignState = f.SignState,
                Amount = f.Amount,
                Freight = f.Amount,
                DocId = f.DocId,
                IsDelete = f.IsDelete,
                IsActive = f.IsActive
            });
            return query.GetCommandText();
        }

        [Benchmark]
        public string SelectEntityTest()
        {
            using IDbConnection connection = new MySqlConnection();
            Query<Order> query = connection.Query<Order>().Where(f => f.Remark.Contains("ACD")).Select(f => new Order { Id = f.Id, Remark = f.Remark, Ignore = f.Ignore, SerialNo = f.SerialNo, IsDelete = f.IsDelete });
            return query.GetCommandText();
        }
    }
}
