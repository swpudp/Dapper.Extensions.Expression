﻿using Dapper.Extensions.Expression;
using Dapper.Extensions.Expression.Queries;
using Dapper.Extensions.Expression.Queries.JoinQueries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;


namespace Dapper.Extensions.Expression.UnitTests.MsSql
{
    /// <summary>
    /// 三表联合查询
    /// </summary>
    [TestClass]
    public class Join3QueryTests : MsSqlBaseTest
    {
        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void JoinQueryContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            query.Where((v, w, x) => v.Remark.Contains("FD2")).Select((a, b, c) => a);
            IEnumerable<Order> data = query.ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void JoinLoopGetCommandTextTest()
        {
            for (int i = 0; i < 10; i++)
            {
                IDbConnection connection = CreateConnection();
                JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                    .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                    .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
                query.Where((v, u, x) => v.Remark.Contains("FD2")).Select((a, b, c) => a);
                string countSql = query.GetCountCommandText();
                Assert.IsNotNull(countSql);
                string commandText = query.GetCommandText();
                Assert.IsNotNull(commandText);
                Console.WriteLine(commandText);
                Console.WriteLine(countSql);
            }
        }

        /// <summary>
        /// 获取sql测试
        /// </summary>
        [TestMethod]
        public void JoinGetLoopCommandTestTest()
        {
            IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            query.Where((v, u, x) => v.Remark.Contains("FD2")).Select((a, b, c) => a);
            for (int i = 0; i < 10; i++)
            {
                string countSql = query.GetCountCommandText();
                Assert.IsNotNull(countSql);

                string commandText = query.GetCommandText();
                Assert.IsNotNull(commandText);

                Console.WriteLine(commandText);
                Console.WriteLine(countSql);
            }
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void JoinQueryWhereCountTest()
        {
            for (int i = 0; i < 10; i++)
            {
                using IDbConnection connection = CreateConnection();
                JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                    .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                    .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
                query.Where((v, p, m) => v.Remark.Contains("FD2")).Select((a, b, x) => a);
                int data = query.Count();
                Assert.IsTrue(data > 0);
            }
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void JoinQueryToListTest()
        {
            for (int i = 0; i < 10; i++)
            {
                using (IDbConnection connection = CreateConnection())
                {
                    JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                        .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                        .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
                    query = query.Where((v, a, x) => v.Remark.Contains("FD2")).Select((a, b, c) => a);
                    IList<Order> data = query.ToList<Order>();
                    Assert.IsTrue(data.Any());
                }
            }
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public async Task JoinQueryWhereCountAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            query.Where((v, v1, v2) => v.Remark.Contains("FD2")).Select((a, b, c) => a);
            int data = await query.CountAsync();
            Assert.IsTrue(data > 0);
        }


        /// <summary>
        /// Contains测试
        /// </summary>
        [TestMethod]
        public async Task JoinQueryToListAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            query.Where((v, x, y) => v.Remark.Contains("FD2")).Select((a, b, c) => a);
            IList<Order> data = await query.ToListAsync<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 实例化数组Contains测试
        /// </summary>
        [TestMethod]
        public void JoinQueryNewArrayContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            query.Where((v, c, w) => new[] { "CDA589B9F0106E7A", "FD094A06D124C5" }.Contains(v.SerialNo));
            IEnumerable<Order> data = query.Select((a, b, c) => a).ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 实例化集合Contains测试
        /// </summary>
        [TestMethod]
        public void JoinQueryNewListContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            query.Where((a, b, c) => new List<string> { "CDA589B9F0106E7A", "FD094A06D124C5" }.Contains(a.SerialNo));
            IEnumerable<Order> data = query.Select((a, b, c) => a).ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void JoinQueryListContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<string> values = new List<string> { "CDA589B9F0106E7A", "FD094A06D124C5" };
            query.Where((a, x, y) => values.Contains(a.SerialNo));
            IEnumerable<Order> data = query.Select((a, b, c) => a).ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void JoinQueryArrayContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            string[] values = new string[] { "CDA589B9F0106E7A", "FD094A06D124C5" };
            query.Where((m, n, p) => values.Contains(m.SerialNo));
            IEnumerable<Order> data = query.Select((a, b, c) => a).ToList<Order>();
            Assert.IsTrue(data.Any());
        }


        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void JoinQueryIEnumerableContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IEnumerable<string> values = new[] { "CDA589B9F0106E7A", "FD094A06D124C5" };
            query.Where((v, x, w) => values.Contains(v.SerialNo));
            IEnumerable<Order> data = query.Select((a, b, c) => a).ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void JoinQueryBoolNotTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            query.Where((v, x, w) => !v.IsDelete);
            IEnumerable<Order> data = query.Select((a, b, c) => a).ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void JoinQueryBoolAccessTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            query.Where((v, x, w) => v.IsDelete);
            IEnumerable<Order> data = query.Select((a, b, c) => a).ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 非空bool类型访问Value属性
        /// </summary>
        [TestMethod]
        public void JoinQueryNullableBoolPropertyOfValueTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            query.Where((v, x, w) => v.IsActive.Value);
            IEnumerable<Order> data = query.Select((a, b, c) => a).ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void JoinQueryParamQueryTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            QueryParam queryParam = new QueryParam
            {
                IsDelete = false,
                CreateTime = DateTime.Now.AddDays(-1)
            };
            query.Where((v, u, w) => v.IsDelete == queryParam.IsDelete && v.CreateTime > queryParam.CreateTime);
            IEnumerable<Order> data = query.Select((a, b, c) => a).ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void JoinQueryMultiOrTest()
        {
            for (int i = 0; i < 10; i++)
            {
                using IDbConnection connection = CreateConnection();
                JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
                QueryParam queryParam = new QueryParam
                {
                    IsDelete = false,
                    CreateTime = DateTime.Now.AddDays(-1),
                    Key = "DB"
                };
                query.Where((v, u, w) => !v.IsDelete && (v.Remark.Contains(queryParam.Key) || v.CreateTime > queryParam.CreateTime));
                IEnumerable<Order> data = query.Select((a, b, c) => a).ToList<Order>();
                Assert.IsTrue(data.Any());
            }
        }

        /// <summary>
        /// 条件块测试
        /// </summary>
        [TestMethod]
        public void JoinQueryConditionBlock1Test()
        {
            using IDbConnection connection = CreateConnection();
            Expression<Func<Order, Item, Buyer, bool>> where = (v, u, x) => v.Status == Status.Running && (v.IsDelete == false || v.Remark.Contains("ab")) && (v.SerialNo.Contains("abc") || v.IsActive == false) && (v.CreateTime > new DateTime(2021, 3, 12) || v.UpdateTime < DateTime.Now.Date);
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            query.Where(where);
            IEnumerable<Order> data = query.Select((a, b, c) => a).ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 生成Guid查询测试
        /// </summary>
        [TestMethod]
        public void JoinQueryNewGuidTest()
        {
            using IDbConnection connection = CreateConnection();
            Expression<Func<Order, Item, Buyer, bool>> where = (v, u, w) => v.Id == Guid.NewGuid();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            query.Where(where);
            IEnumerable<Order> data = query.Select((a, b, c) => a).ToList<Order>();
            Assert.IsFalse(data.Any());
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        [TestMethod]
        public async Task JoinQueryPageTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            query.OrderBy((a, b, c) => a.CreateTime).TakePage(1, 10);
            IList<Order> result = await query.Select((a, b, c) => a).ToListAsync<Order>();
            Assert.AreEqual(10, result.Count);
        }

        /// <summary>
        /// 获取指定数量
        /// </summary>
        [TestMethod]
        public async Task JoinQueryTakeTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IEnumerable<Order> result = await query.OrderBy((a, b, c) => a.CreateTime).Select((a, b, c) => a).Take(100).ToListAsync<Order>();
            Assert.AreEqual(100, result.Count());
        }

        /// <summary>
        /// 排序
        /// </summary>
        [TestMethod]
        public async Task JoinQueryOrderTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            query.TakePage(1, 10).Select((a, b, c) => a).OrderByDescending((a, b, c) => a.CreateTime);
            IList<Order> result = await query.ToListAsync<Order>();
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// 排序分组
        /// </summary>
        [TestMethod]
        public async Task JoinQueryGroupTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            query.TakePage(1, 10).OrderBy((a, b, c) => a.CreateTime).Select((a, b, c) => new { a.CreateTime }).GroupBy((a, b, c) => a.CreateTime);
            IList<Order> result = await query.ToListAsync<Order>();
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// 排序分组
        /// </summary>
        [TestMethod]
        public async Task JoinQueryGroupHavingTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IEnumerable<Order> result = await query.Select((a, b, c) => new { a.CreateTime })
                .GroupBy((f, g, h) => f.CreateTime)
                .Having((f, d, p) => f.CreateTime > new DateTime(2021, 3, 10)).ToListAsync<Order>();
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// 排序分组
        /// </summary>
        [TestMethod]
        public async Task JoinQueryDateTimeDateTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IEnumerable<Order> result = await query.Select((a, b, c) => a).Where((f, d, p) => f.CreateTime.Date > new DateTime(2021, 3, 10).Date).ToListAsync<Order>();
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// 多个排序
        /// </summary>
        [TestMethod]
        public async Task JoinQueryMultiOrderTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            query.TakePage(1, 10).OrderBy((a, b, c) => a.CreateTime);
            IEnumerable<Order> result = await query.Select((a, b, c) => a).ToListAsync<Order>();
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// 多个排序
        /// </summary>
        [TestMethod]
        public async Task JoinQueryMultiOrderAndGroupTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            query.OrderByDescending((a, b, c) => a.CreateTime).OrderBy((a, b, c) => a.SerialNo).GroupBy((am, bm, x) => new { am.CreateTime, am.SerialNo });
            IEnumerable<Order> result = await query.Select((a, b, c) => new { a.CreateTime, a.SerialNo }).ToListAsync<Order>();
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public void JoinSelectObjectTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<OrderAliasModel> models = query.Where((f, g, h) => f.Remark.Contains("AB")).Select((f, g, h) => new
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
            }).ToList<OrderAliasModel>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
            Assert.IsTrue(models.Any(f => f.B.Contains("AB")));
        }

        [TestMethod]
        public void JoinSelectAliasEntityTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<OrderAliasModel> models = query.Where((f, g, h) => f.Remark.Contains("AB")).Select((f, g, h) => new OrderAliasModel
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
            }).ToList<OrderAliasModel>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
            Assert.IsTrue(models.Any(f => f.B.Contains("AB")));
        }

        [TestMethod]
        public void JoinSelectEntityTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<OrderModel> models = query.Where((f, g, h) => f.Remark.Contains("ACD")).Select((f, g, h) => new Order { Id = f.Id, Ignore = f.Ignore, SerialNo = f.SerialNo, Remark = f.Remark, IsActive = f.IsActive, IsDelete = f.IsDelete }).ToList<OrderModel>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any(f => f.Remark.Contains("ACD")));
        }


        [TestMethod]
        public void JoinSelectNullableGuidTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<Guid?> models = query.Where((f, g, h) => f.Remark.Contains("ACD")).Select((f, g, h) => f.DocId).ToList<Guid?>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any(f => !f.HasValue));
        }

        [TestMethod]
        public void JoinSelectDistinctNameTest()
        {

            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<string> models = query.Where((f, g, h) => f.SerialNo == "AE").Select((f, g, h) => f.SerialNo).Distinct().ToList<string>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Count == 1);
        }

        [TestMethod]
        public void JoinEqualsTest()
        {

            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<string> models = query.Where((f, g, h) => f.Remark.Equals("C7")).Select((f, g, h) => f.SerialNo).Distinct().ToList<string>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Count == 1);
        }

        [TestMethod]
        public void JoinGreaterThanTest()
        {

            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<Order> models = query.Select((a, b, c) => a).Where((f, g, h) => f.Version > 10).ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }


        [TestMethod]
        public void JoinExpressionOrderByTest()
        {

            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<Order> models = query.Select((a, b, c) => a).Where((f, g, h) => f.Version > 10).OrderBy((f, g, h) => new { f.SerialNo, f.IsActive }).OrderBy((f, g, h) => f.CreateTime).OrderBy((f, g, h) => new Order { IsDelete = f.IsDelete }).ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void JoinExpressionOrderByDescendingTest()
        {

            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<Order> models = query.Select((a, b, c) => a)
                .Where((f, g, h) => f.Version > 10)
                .OrderBy((f, g, h) => new { f.SerialNo, f.IsDelete, f.IsActive })
                .OrderByDescending((f, g, h) => f.CreateTime)
                .ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void JoinGroupByTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<Order> models = query.Select((a, b, c) => new { a.SerialNo, a.IsActive, a.IsDelete, a.CreateTime })
                .Where((f, g, h) => f.Version > 10)
                .OrderBy((f, g, h) => new { f.SerialNo, f.IsActive })
                .GroupBy((f, g, h) => f.SerialNo).GroupBy((f, g, h) => new { f.IsDelete, f.IsActive })
                .GroupBy((f, g, h) => new Order { CreateTime = f.CreateTime })
                .ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void JoinHavingTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<Order> models = query.Select((a, b, c) => new { a.SerialNo, a.IsDelete, a.IsActive })
                .Where((f, g, h) => f.Version > 10)
                .OrderBy((f, g, h) => new { f.SerialNo, f.IsActive })
                .GroupBy((f, g, h) => f.SerialNo).GroupBy((f, g, h) => new { f.SerialNo, f.IsDelete, f.IsActive }).Having((f, g, h) => f.SerialNo.Contains("A")).ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void JoinWhereIsNullTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<Order> models = query.Select((a, b, c) => a).Where((f, g, h) => f.DocId == null).ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void JoinWhereIsNotNullTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<Order> models = query.Select((a, b, c) => a).Where((f, g, h) => f.DocId != null).ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void JoinMaxStringTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            string max = query.Where((f, g, h) => f.DocId != null).Max((f, g, h) => f.SerialNo);
            Assert.IsNotNull(max);
            Console.WriteLine(max);
        }

        [TestMethod]
        public void JoinMaxIntTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            int max = query.Where((f, g, h) => f.DocId != null).Max((f, g, h) => f.Version);
            Assert.IsTrue(max > 0);
            Console.WriteLine(max);
        }

        [TestMethod]
        public void JoinMaxNullableTest()
        {

            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            int? max = query.Where((f, g, h) => f.DocId == Guid.Empty).Max((f, b, m) => (int?)f.Version);
            Assert.IsFalse(max.HasValue);
        }

        [TestMethod]
        public void JoinMaxStringNullableTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            string max = query.Where((f, g, h) => f.DocId == Guid.Empty).Max((f, g, h) => f.SerialNo);
            Assert.IsNull(max);
        }

        [TestMethod]
        public async Task JoinMaxNullableAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            DateTime? max = await query.Where((f, g, h) => f.DocId == Guid.Empty).MaxAsync((f, g, h) => f.UpdateTime);
            Assert.IsFalse(max.HasValue);
        }

        [TestMethod]
        public async Task JoinMaxNullableNoDataAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            int? max = await query.Where((f, g, h) => f.DocId == Guid.Empty).MaxAsync((f, b, m) => (int?)f.Version);
            Assert.IsFalse(max > 0);
        }


        [TestMethod]
        public void JoinMinIntTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            int max = query.Where((f, g, h) => f.DocId != null).Min((f, g, h) => f.Version);
            Assert.IsTrue(max == 0);
            Console.WriteLine(max);
        }


        [TestMethod]
        public void JoinMinNullableTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            DateTime? max = query.Where((f, g, h) => f.DocId == Guid.Empty).Min((f, g, h) => f.UpdateTime);
            Assert.IsFalse(max.HasValue);
        }

        [TestMethod]
        public async Task JoinMinNullableAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            DateTime? max = await query.Where((f, g, h) => f.DocId == Guid.Empty).MinAsync((f, g, h) => f.UpdateTime);
            Assert.IsFalse(max.HasValue);
        }


        [TestMethod]
        public void JoinSumIntTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            int max = query.Where((f, g, h) => f.DocId != null).Sum((f, g, h) => f.Version);
            Assert.IsTrue(max > 0);
            Console.WriteLine(max);
        }


        [TestMethod]
        public void JoinSumNullableTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            int? max = query.Where((f, g, h) => f.DocId == Guid.Empty).Sum((f, g, h) => (int?)f.Version);
            Assert.IsFalse(max > 0);
        }

        [TestMethod]
        public async Task JoinSumAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            int max = await query.Where((f, g, h) => f.DocId != null).SumAsync((f, g, h) => f.Version);
            Assert.IsTrue(max > 0);
        }

        [TestMethod]
        public void JoinNotSupportTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            Assert.ThrowsException<NotSupportedException>(() => query.Where((f, g, h) => f.DocId == Guid.Empty).Sum((f, g, h) => f.UpdateTime));
        }

        [TestMethod]
        public void JoinDateAddYearsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<Order> entities = query.Where((f, g, h) => f.CreateTime > DateTime.Now.AddYears(-2).AddYears(1)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinDateAddMonthsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<Order> entities = query.Where((f, g, h) => f.CreateTime > DateTime.Now.AddMonths(-2)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinDateAddDaysTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<Order> entities = query.Where((f, g, h) => f.CreateTime > DateTime.Now.AddDays(-20)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinDateAddHoursTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<Order> entities = query.Where((f, g, h) => f.CreateTime > DateTime.Now.AddHours(-500)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinDateAddMinutesTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            DateTime date = DateTime.Now;
            IList<Order> entities = query.Where((f, g, h) => f.CreateTime > date.AddMinutes(-500 * 60)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinDateAddSecondsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            DateTime date = DateTime.Now;
            IList<Order> entities = query.Where((f, g, h) => f.CreateTime > date.AddSeconds(-500 * 60 * 60)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinDateAddMillisecondsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            DateTime date = DateTime.Now;
            IList<Order> entities = query.Where((f, g, h) => f.CreateTime > date.AddMilliseconds(-500 * 60 * 60 * 1000)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinNullableParamTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            DateTime? date = DateTime.Now.AddDays(-20);
            IList<Order> entities = query.Where((f, g, h) => f.CreateTime > date.Value).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinNullableDateTimeValueDateTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            DateTime? date = DateTime.Now.AddDays(-20);
            IList<Order> entities = query.Where((f, g, h) => f.CreateTime > date.Value.Date).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinNotBoolHasValueTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<Order> entities = query.Where((f, g, h) => f.DocId.HasValue).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinBoolNotHasValueTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<Order> entities = query.Where((f, g, h) => !f.DocId.HasValue).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinBoolHasValueTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<Order> entities = query.Where((f, g, h) => f.IsActive.Value).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinNullableBoolEqualsTrueTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<Order> entities = query.Where((f, g, h) => f.IsActive == true).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinNullableBoolEqualsFalseTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<Order> entities = query.Where((f, g, h) => f.IsActive == false).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinNotBoolNotHasValueTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<Order> entities = query.Where((f, g, h) => !f.IsActive.Value).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        /// <summary>
        /// 联合查询
        /// </summary>
        [TestMethod]
        public void JoinTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.Id);
            string commandText = query.GetCommandText();
            Assert.IsNotNull(commandText);
            Console.WriteLine(commandText);
        }

        /// <summary>
        /// 联合查询
        /// </summary>
        [TestMethod]
        public void JoinSelectLeftTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id)
                .Select((m, n, p) => m);
            Console.WriteLine(query.GetCommandText());
        }

        /// <summary>
        /// 联合查询
        /// </summary>
        [TestMethod]
        public void JoinSelectRightTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.Id)
                .On(JoinType.Left, (a, c, d) => c.Id == d.Id)
                .Select((x, y, z) => y);
            Console.WriteLine(query.GetCommandText());
        }

        /// <summary>
        /// 联合查询
        /// </summary>
        [TestMethod]
        public void JoinSelectOtherTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id)
                .Select((x, y, z) => new QueryParam { Key = y.Code, CreateTime = x.CreateTime, IsDelete = x.IsDelete });
            Console.WriteLine(query.GetCommandText());
        }


        [TestMethod]
        public void ToStringTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<Status> statusList = new List<Status> { Status.Running };
            IList<Order> entities = query.Where((f, d, x) => statusList.Contains(f.Status)).Select((f, g, h) => new Order { Id = f.Id, SerialNo = (f.Freight ?? 0).ToString(CultureInfo.InvariantCulture), Remark = f.Remark }).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void ExistTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id);
            IList<Status> statusList = new List<Status> { Status.Running };
            IList<Order> entities = query.Where((f, d, x) => statusList.Contains(f.Status)).Exist<Attachment>((a, b, c, d) => a.Id == d.OrderId && d.Extend == ".pdf").Select((f, g, h) => f).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void PageQueryTest()
        {
            using IDbConnection connection = CreateConnection();
            IList<Status> statusList = new List<Status> { Status.Running };
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id)
                .Where((f, d, x) => statusList.Contains(f.Status))
                .Exist<Attachment>((a, b, c, d) => a.Id == d.OrderId && d.Extend == ".pdf")
                .Select((f, g, h) => f)
                .OrderBy((x, u, z) => x.CreateTime)
                .TakePage(1, 10);
            int count = query.Count();
            Assert.IsTrue(count > 0);
            IList<Order> orders = query.ToList<Order>();
            Assert.IsTrue(orders.Any());
        }

        [TestMethod]
        public void BetweenTest()
        {
            using IDbConnection connection = CreateConnection();
            IList<Status> statusList = new List<Status> { Status.Running };
            JoinQuery<Order, Item, Buyer> query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id)
                .Where((f, d, x) => statusList.Contains(f.Status))
                .Between((a, b, c) => a.Amount, 10m, 20m)
                .OrderByDescending((a, b, c) => a.CreateTime)
                .Select((f, g, h) => f)
                .TakePage(1, 10);
            int count = query.Count();
            Assert.IsTrue(count > 0);
            IList<Order> orders = query.ToList<Order>();
            Assert.IsTrue(orders.Any());
        }

        /// <summary>
        /// whereif测试
        /// </summary>
        [TestMethod]
        public void WhereIfEmptyParamTest()
        {
            QueryParam queryParam = new QueryParam();
            using IDbConnection connection = CreateConnection();
            IQuery query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (c, d, e) => c.BuyerId == e.Id)
                  .WhereIf(queryParam.CreateTime.HasValue, (f, g, j) => f.CreateTime > queryParam.CreateTime)
                  .WhereIf(queryParam.IsDelete == true, (f, i, k) => f.IsDelete)
                  .WhereIf(!string.IsNullOrWhiteSpace(queryParam.Key), (f, k, l) => f.Remark.Contains(queryParam.Key))
                  .Select((g, l, x) => g.Id);
            string commandText = query.GetCommandText();
            Assert.AreEqual("SELECT [t1].[Id] FROM [order] AS [t1] LEFT JOIN [items] AS [t2] ON [t1].[Id] = [t2].[OrderId] LEFT JOIN [buyer] AS [t3] ON [t1].[BuyerId] = [t3].[Id]", commandText.Trim(), true);
        }

        /// <summary>
        /// whereif测试
        /// </summary>
        [TestMethod]
        public void WhereIfFullParamTest()
        {
            QueryParam queryParam = new QueryParam { CreateTime = DateTime.Now.AddDays(-10), IsDelete = true, Key = "1234" };
            using IDbConnection connection = CreateConnection();
            IQuery query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id)
                .WhereIf(queryParam.CreateTime.HasValue, (f, g, j) => f.CreateTime > queryParam.CreateTime)
                .WhereIf(queryParam.IsDelete == true, (f, i, k) => f.IsDelete)
                .WhereIf(!string.IsNullOrWhiteSpace(queryParam.Key), (f, k, l) => f.Remark.Contains(queryParam.Key))
                .Select((g, l, x) => g.Id);
            string commandText = query.GetCommandText();
            Assert.AreEqual("SELECT [t1].[Id] FROM [order] AS [t1] LEFT JOIN [items] AS [t2] ON [t1].[Id] = [t2].[OrderId] LEFT JOIN [buyer] AS [t3] ON [t1].[BuyerId] = [t3].[Id] WHERE [t1].[CreateTime] > @w_p_1 AND [t1].[IsDelete] = @w_p_2 AND [t1].[Remark] LIKE @w_p_3", commandText.Trim(), true);
        }

        /// <summary>
        /// whereif测试
        /// </summary>
        [TestMethod]
        public void WhereIfPartialParamTest()
        {
            QueryParam queryParam = new QueryParam { CreateTime = DateTime.Now.AddDays(-10), IsDelete = true };
            using IDbConnection connection = CreateConnection();
            IQuery query = connection.JoinQuery<Order, Item, Buyer>()
                .On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                .On(JoinType.Left, (a, b, c) => a.BuyerId == c.Id)
                .WhereIf(queryParam.CreateTime.HasValue, (f, g, j) => f.CreateTime > queryParam.CreateTime)
                .WhereIf(queryParam.IsDelete == true, (f, i, k) => f.IsDelete)
                .WhereIf(!string.IsNullOrWhiteSpace(queryParam.Key), (f, k, l) => f.Remark.Contains(queryParam.Key))
                .Select((g, l, x) => g.Id);
            string commandText = query.GetCommandText();
            Assert.AreEqual("SELECT [t1].[Id] FROM [order] AS [t1] LEFT JOIN [items] AS [t2] ON [t1].[Id] = [t2].[OrderId] LEFT JOIN [buyer] AS [t3] ON [t1].[BuyerId] = [t3].[Id] WHERE [t1].[CreateTime] > @w_p_1 AND [t1].[IsDelete] = @w_p_2", commandText.Trim(), true);
        }
    }
}