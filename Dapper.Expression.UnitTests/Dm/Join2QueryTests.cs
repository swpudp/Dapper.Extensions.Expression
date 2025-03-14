﻿using Dapper.Extensions.Expression;
using Dapper.Extensions.Expression.Queries;
using Dapper.Extensions.Expression.Queries.JoinQueries;
using Dapper.Extensions.Expression.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;


namespace Dapper.Extensions.Expression.UnitTests.Dm
{
    [TestClass]
    public class Join2QueryTests : DmBaseTest
    {
        [TestInitialize]
        public void Initialize()
        {
            NamingUtils.SetNamingPolicy(NamingPolicy.UpperCase);
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void JoinQueryContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            query.Where((v, w) => v.SerialNo.Contains("FD2"));
            IEnumerable<Order> data = query.ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void QuerySegregateTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            query.Where((v, w) => (v.SignState == SignState.Signed || (v.SignState == SignState.UnSign && w.Price > 20m)) && v.IsActive.Value);
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
                JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
                query.Where((v, u) => v.SerialNo.Contains("FD2")).Select((a, b) => a);
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
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            query.Where((v, u) => v.SerialNo.Contains("FD2")).Select((a, b) => a);
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
                JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
                query.Where((v, p) => v.SerialNo.Contains("FD2")).Select((a, b) => a);
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
                    JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
                    query = query.Where((v, a) => v.SerialNo.Contains("FD2")).Select((a, b) => a);
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
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            query.Where((v, v1) => v.SerialNo.Contains("FD2")).Select((a, b) => a);
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
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            query.Where((v, x) => v.SerialNo.Contains("FD2")).Select((a, b) => a);
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
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            query.Where((v, c) => new[] { "0C53AEDA46DC957B9EDD", "70F266168B87F90A972C" }.Contains(v.SerialNo));
            IEnumerable<Order> data = query.Select((a, b) => a).ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 实例化集合Contains测试
        /// </summary>
        [TestMethod]
        public void JoinQueryNewListContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            query.Where((a, b) => new List<string> { "0C53AEDA46DC957B9EDD", "70F266168B87F90A972C" }.Contains(a.SerialNo));
            IEnumerable<Order> data = query.Select((a, b) => a).ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void JoinQueryListContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<string> values = new List<string> { "0C53AEDA46DC957B9EDD", "70F266168B87F90A972C" };
            query.Where((a, x) => values.Contains(a.SerialNo));
            IEnumerable<Order> data = query.Select((a, b) => a).ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void JoinQueryArrayContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            string[] values = { "556761", "3D04" };
            query.Where((m, n) => values.Contains(m.SerialNo));
            IEnumerable<Order> data = query.Select((a, b) => a).ToList<Order>();
            Assert.IsTrue(data.Any());
        }


        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void JoinQueryIEnumerableContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IEnumerable<string> values = new[] { "556761", "3D04" };
            query.Where((v, x) => values.Contains(v.SerialNo));
            IEnumerable<Order> data = query.Select((a, b) => a).ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void JoinQueryBoolNotTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            query.Where((v, x) => !v.IsDelete);
            IEnumerable<Order> data = query.Select((a, b) => a).ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void JoinQueryBoolAccessTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            query.Where((v, x) => v.IsDelete);
            IEnumerable<Order> data = query.Select((a, b) => a).ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 非空bool类型访问Value属性
        /// </summary>
        [TestMethod]
        public void JoinQueryNullableBoolPropertyOfValueTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            query.Where((v, x) => v.IsActive.Value);
            IEnumerable<Order> data = query.Select((a, b) => a).ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void JoinQueryParamQueryTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            QueryParam queryParam = new QueryParam
            {
                IsDelete = false,
                CreateTime = DateTime.Now.AddDays(-1)
            };
            query.Where((v, u) => v.IsDelete == queryParam.IsDelete && v.CreateTime > queryParam.CreateTime);
            IEnumerable<Order> data = query.Select((a, b) => a).ToList<Order>();
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
                JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
                QueryParam queryParam = new QueryParam
                {
                    IsDelete = false,
                    CreateTime = DateTime.Now.AddDays(-1),
                    Key = "DB"
                };
                query.Where((v, u) => !v.IsDelete && (v.SerialNo.Contains(queryParam.Key) || v.CreateTime > queryParam.CreateTime));
                IEnumerable<Order> data = query.Select((a, b) => a).ToList<Order>();
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
            Expression<Func<Order, Item, bool>> where = (v, u) => v.Status == Status.Running && (v.IsDelete == false || v.Remark.Contains("ab")) && (v.SerialNo.Contains("abc") || v.SerialNo == "ab") && (v.CreateTime > new DateTime(2021, 3, 12) || v.UpdateTime < DateTime.Now.Date);
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            query.Where(where);
            IEnumerable<Order> data = query.Select((a, b) => a).ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 生成Guid查询测试
        /// </summary>
        [TestMethod]
        public void JoinQueryNewGuidTest()
        {
            using IDbConnection connection = CreateConnection();
            Expression<Func<Order, Item, bool>> where = (v, u) => v.Id == Guid.NewGuid();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            query.Where(where);
            IEnumerable<Order> data = query.Select((a, b) => a).ToList<Order>();
            Assert.IsFalse(data.Any());
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        [TestMethod]
        public async Task JoinQueryPageTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            query.TakePage(1, 10);
            IList<Order> result = await query.Select((a, b) => a).ToListAsync<Order>();
            Assert.AreEqual(10, result.Count);
        }

        /// <summary>
        /// 获取指定数量
        /// </summary>
        [TestMethod]
        public async Task JoinQueryTakeTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IEnumerable<Order> result = await query.Select((a, b) => a).Take(100).ToListAsync<Order>();
            Assert.AreEqual(100, result.Count());
        }

        /// <summary>
        /// 排序
        /// </summary>
        [TestMethod]
        public async Task JoinQueryOrderTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            query.TakePage(1, 10).Select((a, b) => a).OrderByDescending((a, b) => a.CreateTime);
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
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            query.TakePage(1, 10).Select((a, b) => a).GroupBy((a, b) => a.CreateTime);
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
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IEnumerable<Order> result = await query.Select((a, b) => a).GroupBy((f, g) => f.CreateTime).Having((f, d) => f.CreateTime > new DateTime(2021, 3, 10)).ToListAsync<Order>();
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// 排序分组
        /// </summary>
        [TestMethod]
        public async Task JoinQueryDateTimeDateTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IEnumerable<Order> result = await query.Select((a, b) => a).Where((f, d) => f.CreateTime.Date > new DateTime(2021, 3, 10).Date).ToListAsync<Order>();
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// 多个排序
        /// </summary>
        [TestMethod]
        public async Task JoinQueryMultiOrderTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            query.TakePage(1, 10).OrderBy((a, b) => a.CreateTime);
            IEnumerable<Order> result = await query.Select((a, b) => a).ToListAsync<Order>();
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// 多个排序
        /// </summary>
        [TestMethod]
        public async Task JoinQueryMultiOrderAndGroupTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            query.TakePage(1, 10).OrderByDescending((a, b) => a.CreateTime).OrderBy((a, b) => a.SerialNo).GroupBy((am, bm) => am.CreateTime);
            IEnumerable<Order> result = await query.Select((a, b) => a).ToListAsync<Order>();
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public void JoinSelectObjectTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<OrderAliasModel> models = query.Where((f, g) => f.SerialNo.Contains("AB")).Select((f, g) => new
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
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<OrderAliasModel> models = query.Where((f, g) => f.SerialNo.Contains("AB")).Select((f, g) => new OrderAliasModel
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
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<OrderModel> models = query.Where((f, g) => f.SerialNo.Contains("ACD")).Select((f, g) => new OrderModel { Id = f.Id, Ignore = f.Ignore, Number = f.SerialNo, CreateTime = f.CreateTime, IsDelete = f.IsDelete }).ToList<OrderModel>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any(f => f.Number.Contains("ACD")));
        }


        [TestMethod]
        public void JoinSelectNullableGuidTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Guid?> models = query.Where((f, g) => f.SerialNo.Contains("ACD")).Select((f, g) => f.DocId).ToList<Guid?>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any(f => !f.HasValue));
        }

        [TestMethod]
        public void JoinSelectDistinctNameTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<string> models = query.Where((f, g) => f.SerialNo == "A82639").Select((f, g) => f.SerialNo).Distinct().ToList<string>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
            Assert.IsTrue(models.Count == 1);
        }

        [TestMethod]
        public void JoinEqualsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<string> models = query.Where((f, g) => f.SerialNo.Equals("A82639")).Select((f, g) => f.SerialNo).Distinct().ToList<string>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Count == 1);
        }

        [TestMethod]
        public void JoinGreaterThanTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Order> models = query.Select((a, b) => a).Where((f, g) => f.Version > 10).ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }


        [TestMethod]
        public void JoinExpressionOrderByTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Order> models = query.Select((a, b) => a).Where((f, g) => f.Version > 10).OrderBy((f, g) => new { f.SerialNo, f.IsActive }).OrderBy((f, g) => f.CreateTime).OrderBy((f, g) => new Order { IsDelete = f.IsDelete }).ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void JoinExpressionOrderByDescendingTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Order> models = query.Select((a, b) => a).Where((f, g) => f.Version > 10).OrderBy((f, g) => new { f.SerialNo, f.IsDelete, f.IsActive }).OrderByDescending((f, g) => f.CreateTime).OrderByDescending((f, g) => new Order { IsDelete = f.IsDelete, IsActive = f.IsActive }).ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        /// <summary>
        /// 按常量排序
        /// </summary>
        [TestMethod]
        public void JoinOrderByConstTest()
        {
            using IDbConnection connection = CreateConnection();
            QueryParam queryParam1 = new QueryParam { Key = "CreateTime" };
            QueryParam queryParam2 = new QueryParam { Key = "Price" };
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            query.Where((f, g) => !f.IsDelete)
                  .OrderBy(queryParam1.Key)
                  .OrderBy(queryParam2.Key)
                  .OrderBy("Amount")
                  .OrderBy("Discount")
                  .OrderBy((f, g) => f.Index)
                  .OrderBy((f, g) => g.Code);
            Order order = query.FirstOrDefault<Order>();
            Assert.IsNotNull(order);
            //ORDER BY `t1`.`CreateTime` ASC ,`t2`.`Price` ASC ,`t1`.`Amount` ASC ,`t2`.`Discount` ASC ,`t1`.`Index` ASC ,`t2`.`Code` ASC  
        }

        /// <summary>
        /// 按常量排序
        /// </summary>
        [TestMethod]
        public void JoinOrderByNewObjectTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            query.Where((f, g) => !f.IsDelete)
                  .OrderBy((_, _) => new QueryParam { Key = "CreateTime" })
                  .OrderBy((_, _) => new QueryParam { Key = "Price" });
            Order order = query.FirstOrDefault<Order>();
            Assert.IsNotNull(order);
            //ORDER BY `t1`.`CreateTime` ASC ,`t2`.`Price` ASC ,`t1`.`Amount` ASC ,`t2`.`Discount` ASC ,`t1`.`Index` ASC ,`t2`.`Code` ASC  
        }

        [TestMethod]
        public void JoinGroupByTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Order> models = query.Select((a, b) => a).Where((f, g) => f.Version > 10).OrderBy((f, g) => new { f.SerialNo, f.IsActive }).GroupBy((f, g) => f.SerialNo).GroupBy((f, g) => new { f.IsDelete }).GroupBy((f, g) => new Order { CreateTime = f.CreateTime }).ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void JoinHavingTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Order> models = query.Select((a, b) => a).Where((f, g) => f.Version > 10).OrderBy((f, g) => new { f.SerialNo, f.IsActive }).GroupBy((f, g) => f.SerialNo).GroupBy((f, g) => new { f.IsDelete }).Having((f, g) => f.Version > 5).ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void JoinWhereIsNullTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Order> models = query.Select((a, b) => a).Where((f, g) => f.DocId == null).ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void JoinWhereIsNotNullTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Order> models = query.Select((a, b) => a).Where((f, g) => f.DocId != null).ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void JoinMaxStringTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            string max = query.Where((f, g) => f.DocId != null).Max((f, g) => f.SerialNo);
            Assert.IsNotNull(max);
            Console.WriteLine(max);
        }

        [TestMethod]
        public void JoinMaxIntTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            int max = query.Where((f, g) => f.DocId != null).Max((f, g) => f.Version);
            Assert.IsTrue(max > 0);
            Console.WriteLine(max);
        }

        [TestMethod]
        public void JoinMaxNullableTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            int? max = query.Where((f, g) => f.DocId != null).Max((f, b) => (int?)f.Version);
            Assert.IsFalse(max.HasValue);
        }

        [TestMethod]
        public void JoinMaxStringNullableTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            string max = query.Where((f, g) => f.DocId != null).Max((f, g) => f.SerialNo);
            Assert.IsNull(max);
        }

        [TestMethod]
        public async Task JoinMaxNullableAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            DateTime? max = await query.Where((f, g) => f.DocId != null).MaxAsync((f, g) => f.UpdateTime);
            Assert.IsFalse(max.HasValue);
        }

        [TestMethod]
        public async Task JoinMaxNullableNoDataAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            int? max = await query.Where((f, g) => f.DocId != null).MaxAsync((f, b) => (int?)f.Version);
            Assert.IsFalse(max > 0);
        }


        [TestMethod]
        public void JoinMinIntTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            int max = query.Where((f, g) => f.DocId != null).Min((f, g) => f.Version);
            Assert.IsTrue(max == 0);
            Console.WriteLine(max);
        }


        [TestMethod]
        public void JoinMinNullableTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            DateTime? max = query.Where((f, g) => f.DocId != null).Min((f, g) => f.UpdateTime);
            Assert.IsFalse(max.HasValue);
        }

        [TestMethod]
        public async Task JoinMinNullableAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            DateTime? max = await query.Where((f, g) => f.DocId != null).MinAsync((f, g) => f.UpdateTime);
            Assert.IsFalse(max.HasValue);
        }


        [TestMethod]
        public void JoinSumIntTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            int max = query.Where((f, g) => f.DocId != null).Sum((f, g) => f.Version);
            Assert.IsTrue(max > 0);
            Console.WriteLine(max);
        }


        [TestMethod]
        public void JoinSumNullableTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            int? max = query.Where((f, g) => f.DocId != null).Sum((f, g) => (int?)f.Version);
            Assert.IsFalse(max > 0);
        }

        [TestMethod]
        public async Task JoinSumAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            int max = await query.Where((f, g) => f.DocId != null).SumAsync((f, g) => f.Version);
            Assert.IsTrue(max > 0);
        }

        [TestMethod]
        public void JoinNotSupportTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            Assert.ThrowsException<NotSupportedException>(() => query.Where((f, g) => f.DocId != null).Sum((f, g) => f.UpdateTime));
        }

        [TestMethod]
        public void JoinDateAddYearsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Order> entities = query.Where((f, g) => f.CreateTime > DateTime.Now.AddYears(-2).AddYears(1)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinDateAddMonthsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Order> entities = query.Where((f, g) => f.CreateTime > DateTime.Now.AddMonths(-2)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinDateAddDaysTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Order> entities = query.Where((f, g) => f.CreateTime > DateTime.Now.AddDays(-20)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinDateAddHoursTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Order> entities = query.Where((f, g) => f.CreateTime > DateTime.Now.Date.AddHours(-500)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinDateAddMinutesTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            DateTime date = DateTime.Now;
            IList<Order> entities = query.Where((f, g) => f.CreateTime > date.Date.AddMinutes(-500 * 60)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinDateAddSecondsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            DateTime date = DateTime.Now;
            IList<Order> entities = query.Where((f, g) => f.CreateTime > date.Date.AddSeconds(-500 * 60 * 60)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinDateAddMillisecondsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            DateTime date = DateTime.Now;
            IList<Order> entities = query.Where((f, g) => f.CreateTime > date.Date.AddMilliseconds(-500 * 60 * 60 * 1000)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinNullableParamTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            DateTime? date = DateTime.Now.AddDays(-20);
            IList<Order> entities = query.Where((f, g) => f.CreateTime > date.Value).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void JoinNullableDateTimeValueDateTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            DateTime? date = DateTime.Now.AddDays(-20);
            IList<Order> entities = query.Where((f, g) => f.CreateTime > date.Value.Date).ToList<Order>();
            Assert.IsTrue(entities.Any());
            Assert.IsTrue(entities.All(f => f.CreateTime > date.Value.Date));
        }

        [TestMethod]
        public void JoinNotBoolHasValueTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Order> entities = query.Where((f, g) => f.DocId.HasValue).ToList<Order>();
            Assert.IsTrue(entities.Any());
            Assert.IsTrue(entities.All(f => f.DocId.HasValue));
        }

        [TestMethod]
        public void JoinBoolNotHasValueTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Order> entities = query.Where((f, g) => !f.DocId.HasValue).Take(10).ToList<Order>();
            Assert.IsTrue(entities.Any());
            Assert.IsTrue(entities.All(f => !f.DocId.HasValue));
        }

        [TestMethod]
        public void JoinBoolHasValueTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Order> entities = query.Where((f, g) => f.IsActive.Value).ToList<Order>();
            Assert.IsTrue(entities.Any());
            Assert.IsTrue(entities.All(f => f.IsActive == true));
        }

        [TestMethod]
        public void JoinNullableBoolEqualsTrueTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Order> entities = query.Where((f, g) => f.IsActive == true).ToList<Order>();
            Assert.IsTrue(entities.Any());
            Assert.IsTrue(entities.All(f => f.IsActive == true));
        }

        [TestMethod]
        public void JoinNullableBoolEqualsFalseTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Order> entities = query.Where((f, g) => f.IsActive == false).ToList<Order>();
            Assert.IsTrue(entities.Any());
            Assert.IsTrue(entities.All(f => f.IsActive == false));
        }

        [TestMethod]
        public void JoinNotBoolNotHasValueTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Order> entities = query.Where((f, g) => !f.IsActive.Value).ToList<Order>();
            Assert.IsTrue(entities.Any());
            Assert.IsTrue(entities.All(f => f.IsActive == false));
        }

        /// <summary>
        /// 联合查询
        /// </summary>
        [TestMethod]
        public void JoinTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
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
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId).Select((m, n) => m);
            Console.WriteLine(query.GetCommandText());
        }

        /// <summary>
        /// 联合查询
        /// </summary>
        [TestMethod]
        public void JoinSelectRightTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId).Select((x, y) => y);
            Console.WriteLine(query.GetCommandText());
        }

        /// <summary>
        /// 联合查询
        /// </summary>
        [TestMethod]
        public void JoinSelectOtherTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId).Select((x, y) => new QueryParam { Key = y.Code, CreateTime = x.UpdateTime, IsDelete = x.IsDelete });
            Console.WriteLine(query.GetCommandText());
        }


        [TestMethod]
        public void ToStringTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Status> statusList = new List<Status> { Status.Running };
            IList<Order> entities = query.Where((f, d) => statusList.Contains(f.Status)).Select((f, g) => new Order { Id = f.Id, Status = f.Status, SerialNo = (f.Freight ?? 0).ToString(CultureInfo.InvariantCulture), Remark = f.SerialNo }).ToList<Order>();
            Assert.IsTrue(entities.Any());
            Assert.IsTrue(entities.All(f => f.Status == Status.Running));
        }


        [TestMethod]
        public void ExistsTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Status> statusList = new List<Status> { Status.Running };
            IList<Order> orders = query.Where((f, d) => statusList.Contains(f.Status)).Exist<Buyer>((a, b, c) => a.BuyerId == c.Id && c.Code.Equals("E24C")).Select((f, g) => new Order { Id = f.Id, SerialNo = (f.Freight ?? 0).ToString(CultureInfo.InvariantCulture), Remark = f.SerialNo }).ToList<Order>();
            Assert.IsTrue(orders.Any());
        }

        [TestMethod]
        public void PageQueryTest()
        {
            using IDbConnection connection = CreateConnection();
            IList<Status> statusList = new List<Status> { Status.Running };
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId).Where((f, d) => statusList.Contains(f.Status)).Exist<Buyer>((a, b, c) => a.BuyerId == c.Id && c.Code.Equals("E24C")).Select((f, g) => f).TakePage(1, 10);
            int count = query.Count();
            Assert.IsTrue(count > 0);
            IList<Order> orders = query.ToList<Order>();
            Assert.IsTrue(orders.Any());
        }


        [TestMethod]
        public void BetweenTest()
        {
            using IDbConnection connection = CreateConnection();
            JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
            IList<Status> statusList = new List<Status> { Status.Running };
            IList<Order> orders = query.Where((f, d) => statusList.Contains(f.Status)).Between((a, b) => a.Amount, 10m, 20m).ToList<Order>();
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
            IQuery query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId)
                  .WhereIf(queryParam.CreateTime.HasValue, (f, g) => f.CreateTime > queryParam.CreateTime)
                  .WhereIf(queryParam.IsDelete == true, (f, i) => f.IsDelete)
                  .WhereIf(!string.IsNullOrWhiteSpace(queryParam.Key), (f, k) => f.Remark.Contains(queryParam.Key))
                  .Select((g, l) => g.Id);
            string commandText = query.GetCommandText();
            Assert.AreEqual("SELECT `t1`.`Id` FROM `order` AS `t1` LEFT JOIN `items` AS `t2` ON `t1`.`Id` = `t2`.`OrderId`", commandText.Trim(), true);
        }

        /// <summary>
        /// whereif测试
        /// </summary>
        [TestMethod]
        public void WhereIfFullParamTest()
        {
            QueryParam queryParam = new QueryParam { CreateTime = DateTime.Now.AddDays(-10), IsDelete = true, Key = "1234" };
            using IDbConnection connection = CreateConnection();
            IQuery query = connection.JoinQuery<Order, Item>()
                  .On(JoinType.Left, (x, y) => x.Id == y.OrderId)
                  .WhereIf(queryParam.CreateTime.HasValue, (f, f1) => f.CreateTime > queryParam.CreateTime)
                  .WhereIf(queryParam.IsDelete == true, (f, f2) => f.IsDelete)
                  .WhereIf(!string.IsNullOrWhiteSpace(queryParam.Key), (f, f3) => f.Remark.Contains(queryParam.Key))
                  .Select((g, g1) => g.Id);
            string commandText = query.GetCommandText();
            Assert.AreEqual("SELECT `t1`.`Id` FROM `order` AS `t1` LEFT JOIN `items` AS `t2` ON `t1`.`Id` = `t2`.`OrderId` WHERE `t1`.`CreateTime` > @w_p_1 AND `t1`.`IsDelete` = @w_p_2 AND `t1`.`Remark` LIKE @w_p_3", commandText.Trim(), true);
        }

        /// <summary>
        /// whereif测试
        /// </summary>
        [TestMethod]
        public void WhereIfPartialParamTest()
        {
            QueryParam queryParam = new QueryParam { CreateTime = DateTime.Now.AddDays(-10), IsDelete = true };
            using IDbConnection connection = CreateConnection();
            IQuery query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (x, y) => x.Id == y.OrderId)
                .WhereIf(queryParam.CreateTime.HasValue, (f, f1) => f.CreateTime > queryParam.CreateTime)
                .WhereIf(queryParam.IsDelete == true, (f, f2) => f.IsDelete)
                .WhereIf(!string.IsNullOrWhiteSpace(queryParam.Key), (f, f3) => f.Remark.Contains(queryParam.Key))
                .Select((g, g1) => g.Id);
            string commandText = query.GetCommandText();
            Assert.AreEqual("SELECT `t1`.`Id` FROM `order` AS `t1` LEFT JOIN `items` AS `t2` ON `t1`.`Id` = `t2`.`OrderId` WHERE `t1`.`CreateTime` > @w_p_1 AND `t1`.`IsDelete` = @w_p_2", commandText.Trim(), true);
        }
    }
}