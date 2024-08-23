using Dapper.Extensions.Expression;
using Dapper.Extensions.Expression.Queries;
using Dapper.Extensions.Expression.Queries.JoinQueries;
using Dapper.Extensions.Expression.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;


namespace Dapper.Extensions.Expression.UnitTests.MySql
{
    [TestClass]
    public class QueryTests : MysqlBaseTest
    {
        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void QueryContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            query.Where(v => v.Remark.Contains("FD2"));
            IEnumerable<Order> data = query.ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void LoopGetCommandTextTest()
        {
            for (int i = 0; i < 10; i++)
            {
                IDbConnection connection = CreateConnection();
                Query<Order> query = connection.Query<Order>();
                query.Where(v => v.Remark.Contains("FD2"));
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
        public void GetLoopCommandTestTest()
        {
            IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            query.Where(v => v.Remark.Contains("FD2"));
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
        public void QueryWhereCountTest()
        {
            for (int i = 0; i < 10; i++)
            {
                using IDbConnection connection = CreateConnection();
                Query<Order> query = connection.Query<Order>();
                query.Where(v => v.Remark.Contains("FD2"));
                int data = query.Count();
                Assert.IsTrue(data > 0);
            }
        }


        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void QueryToListTest()
        {
            for (int i = 0; i < 10; i++)
            {
                using (IDbConnection connection = CreateConnection())
                {
                    Query<Order> query = connection.Query<Order>();
                    query = query.Where(v => v.Remark.Contains("FD2"));
                    IList<Order> data = query.ToList<Order>();
                    Assert.IsTrue(data.Any());
                }
            }
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public async Task QueryWhereCountAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            query.Where(v => v.Remark.Contains("FD2"));
            int data = await query.CountAsync();
            Assert.IsTrue(data > 0);
        }


        /// <summary>
        /// Contains测试
        /// </summary>
        [TestMethod]
        public async Task QueryToListAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            query.Where(v => v.Remark.Contains("FD2"));
            IList<Order> data = await query.ToListAsync<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 实例化数组Contains测试
        /// </summary>
        [TestMethod]
        public void QueryNewArrayContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            query.Where(v => new[] { "A82639", "8064C0A3" }.Contains(v.SerialNo));
            IEnumerable<Order> data = query.ToList<Order>();
            Assert.IsTrue(data.Any());
            Assert.IsTrue(data.All(d => new[] { "A82639", "8064C0A3" }.Any(f => f == d.SerialNo)));
        }

        /// <summary>
        /// 实例化集合Contains测试
        /// </summary>
        [TestMethod]
        public void QueryNewListContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            query.Where(v => new List<string> { "A82639", "8064C0A3" }.Contains(v.SerialNo));
            IEnumerable<Order> data = query.ToList<Order>();
            Assert.IsTrue(data.Any());
            Assert.IsTrue(data.All(d => new List<string> { "A82639", "8064C0A3" }.Any(f => f == d.SerialNo)));
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void QueryListContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<string> values = new List<string> { "A82639", "8064C0A3" };
            query.Where(v => values.Contains(v.SerialNo));
            IEnumerable<Order> data = query.ToList<Order>();
            Assert.IsTrue(data.Any());
            Assert.IsTrue(data.All(d => values.Any(f => f == d.SerialNo)));
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void QueryArrayContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            string[] values = { "ABC", "BCD" };
            Query<Order> query = connection.Query<Order>().Where(v => values.Contains(v.Remark));
            IEnumerable<Order> data = query.ToList<Order>();
            Assert.IsTrue(data.Any());
            Assert.IsTrue(data.All(d => values.Any(f => f == d.SerialNo)));
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void QueryIEnumerableContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            IEnumerable<string> values = new[] { "EFC", "C0A3" };
            Query<Order> query = connection.Query<Order>().Where(v => values.Contains(v.SerialNo));
            IEnumerable<Order> data = query.ToList<Order>();
            Assert.IsTrue(data.Any());
            Assert.IsTrue(data.All(d => values.Any(f => f == d.SerialNo)));
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void QueryBoolNotTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            query.Where(v => !v.IsDelete);
            IEnumerable<Order> data = query.ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void QueryBoolAccessTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            query.Where(v => v.IsDelete);
            IEnumerable<Order> data = query.ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void QueryParamQueryTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            QueryParam queryParam = new QueryParam
            {
                IsDelete = false,
                CreateTime = DateTime.Now.AddDays(-1)
            };
            query.Where(v => v.IsDelete == queryParam.IsDelete && v.CreateTime > queryParam.CreateTime);
            IEnumerable<Order> data = query.ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void QueryMultiOrTest()
        {
            for (int i = 0; i < 10; i++)
            {
                using IDbConnection connection = CreateConnection();
                Query<Order> query = connection.Query<Order>();
                QueryParam queryParam = new QueryParam
                {
                    IsDelete = false,
                    CreateTime = new DateTime(2021, 3, 20),
                    Key = "DB"
                };
                query.Where(v => !v.IsDelete && (v.Remark.Contains(queryParam.Key) || v.CreateTime > queryParam.CreateTime));
                IEnumerable<Order> data = query.ToList<Order>();
                Assert.IsTrue(data.Any());
            }
        }

        /// <summary>
        /// 条件块测试
        /// </summary>
        [TestMethod]
        public void QueryConditionBlock1Test()
        {
            using IDbConnection connection = CreateConnection();
            Expression<Func<Order, bool>> where = v => v.Status == Status.Draft && (v.IsDelete == false || v.Remark.Contains("FD")) && (v.SerialNo.Contains("FD") || v.SerialNo == "GD") && (v.CreateTime > new DateTime(2021, 3, 12) || v.UpdateTime < DateTime.Now.Date);
            Query<Order> query = connection.Query<Order>();
            query.Where(where);
            IEnumerable<Order> data = query.ToList<Order>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 生成Guid查询测试
        /// </summary>
        [TestMethod]
        public void QueryNewGuidTest()
        {
            using IDbConnection connection = CreateConnection();
            Expression<Func<Order, bool>> where = v => v.Id == Guid.NewGuid();
            Query<Order> query = connection.Query<Order>();
            query.Where(where);
            IEnumerable<Order> data = query.ToList<Order>();
            Assert.IsFalse(data.Any());
        }

        /// <summary>
        /// 获取单个测试
        /// </summary>
        [TestMethod]
        public void GetTest()
        {
            using IDbConnection connection = CreateConnection();
            Order testEntity = connection.Get<Order>(f => f.Id == Guid.Parse("001399e7-cacf-4323-8f18-75a9ef1480e0"));
            Assert.IsNotNull(testEntity);
        }

        /// <summary>
        /// 获取单个测试
        /// </summary>
        [TestMethod]
        public async Task GetAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            Order order = await connection.GetAsync<Order>(f => f.Id == Guid.Parse("001399e7-cacf-4323-8f18-75a9ef1480e0"));
            Assert.IsNotNull(order);
        }

        /// <summary>
        /// 获取单个测试
        /// </summary>
        [TestMethod]
        public async Task GetNewGuidAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            Order testEntity = await connection.GetAsync<Order>(f => f.Id == Guid.NewGuid());
            Assert.IsNull(testEntity);
        }

        /// <summary>
        /// 获取单个测试
        /// </summary>
        [TestMethod]
        public void FirstOrDefaultTest()
        {
            using IDbConnection connection = CreateConnection();
            Order testEntity = connection.Query<Order>().Where(f => f.Remark.Contains("CDF")).FirstOrDefault<Order>();
            Assert.IsNotNull(testEntity);
        }

        /// <summary>
        /// 获取单个测试
        /// </summary>
        [TestMethod]
        public void FirstOrDefaultToModelTest()
        {
            using IDbConnection connection = CreateConnection();
            OrderModel testEntity = connection.Query<Order>().Where(f => f.Remark.Contains("CDF")).FirstOrDefault<OrderModel>();
            Assert.IsNotNull(testEntity);
        }

        /// <summary>
        /// 获取单个测试
        /// </summary>
        [TestMethod]
        public async Task FirstOrDefaultAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            Order testEntity = await connection.Query<Order>().Where(f => f.Remark.Contains("CDF")).FirstOrDefaultAsync<Order>();
            Assert.IsNotNull(testEntity);
        }


        /// <summary>
        /// 获取单个测试
        /// </summary>
        [TestMethod]
        public void AnyTest()
        {
            using IDbConnection connection = CreateConnection();
            bool result = connection.Query<Order>().Where(f => f.Remark.Contains("CDF")).Any();
            Assert.IsTrue(result);
        }

        /// <summary>
        /// 获取单个测试
        /// </summary>
        [TestMethod]
        public async Task AnyAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            bool result = await connection.Query<Order>().Where(f => f.Remark.Contains("CDF")).AnyAsync();
            Assert.IsTrue(result);
        }

        /// <summary>
        /// 获取全部测试
        /// </summary>
        [TestMethod]
        public async Task GetAllAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            IEnumerable<Order> testEntity = await connection.GetAllAsync<Order>();
            Assert.IsNotNull(testEntity);
            Assert.IsTrue(testEntity.Any());
        }

        /// <summary>
        /// 获取全部测试
        /// </summary>
        [TestMethod]
        public void GetAllTest()
        {
            using IDbConnection connection = CreateConnection();
            IEnumerable<Order> testEntity = connection.GetAll<Order>();
            Assert.IsNotNull(testEntity);
            Assert.IsTrue(testEntity.Any());
        }

        /// <summary>
        /// 获取数量测试
        /// </summary>
        [TestMethod]
        public void GetCountNoDataTest()
        {
            using IDbConnection connection = CreateConnection();
            int total = connection.GetCount<Emit>();
            Assert.IsFalse(total > 0);
            Assert.AreEqual(0, total);
        }

        /// <summary>
        /// 获取数量测试
        /// </summary>
        [TestMethod]
        public void GetCountExistDataTest()
        {
            using IDbConnection connection = CreateConnection();
            int total = connection.GetCount<Order>();
            Assert.IsTrue(total > 0);
        }

        /// <summary>
        /// 获取数量测试
        /// </summary>
        [TestMethod]
        public async Task GetCountAsyncNoDataTest()
        {
            using IDbConnection connection = CreateConnection();
            int total = await connection.GetCountAsync<Emit>();
            Assert.IsFalse(total > 0);
            Assert.AreEqual(0, total);
        }

        /// <summary>
        /// 获取数量测试
        /// </summary>
        [TestMethod]
        public async Task GetCountAsyncExistDataTest()
        {
            using IDbConnection connection = CreateConnection();
            int total = await connection.GetCountAsync<Order>();
            Assert.IsTrue(total > 0);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        [TestMethod]
        public async Task QueryPageTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            query.TakePage(1, 10);
            IList<Order> result = await query.ToListAsync<Order>();
            Assert.AreEqual(10, result.Count);
        }

        /// <summary>
        /// 获取指定数量
        /// </summary>
        [TestMethod]
        public async Task QueryTakeTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IEnumerable<Order> result = await query.Take(100).ToListAsync<Order>();
            Assert.AreEqual(100, result.Count());
        }

        /// <summary>
        /// 排序
        /// </summary>
        [TestMethod]
        public async Task QueryOrderTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            query.TakePage(1, 10).OrderByDescending(f => f.CreateTime);
            IList<Order> result = await query.ToListAsync<Order>();
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// 排序分组
        /// </summary>
        [TestMethod]
        public async Task QueryGroupTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>().TakePage(1, 10).GroupBy(f => f.Status);
            IList<Order> result = await query.ToListAsync<Order>();
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// 排序分组
        /// </summary>
        [TestMethod]
        public async Task QueryGroupHavingTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IEnumerable<Order> result = await query.GroupBy(f => f.Status).Having(f => f.Version > 1).ToListAsync<Order>();
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// 排序分组
        /// </summary>
        [TestMethod]
        public async Task QueryDateTimeDateTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IEnumerable<Order> result = await query.Where(f => f.CreateTime.Date > new DateTime(2021, 3, 10).Date).ToListAsync<Order>();
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// 多个排序
        /// </summary>
        [TestMethod]
        public async Task QueryMultiOrderTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            query.TakePage(1, 10).OrderByDescending(f => f.CreateTime).OrderBy(f => f.SerialNo);
            IEnumerable<Order> result = await query.ToListAsync<Order>();
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// 多个排序
        /// </summary>
        [TestMethod]
        public async Task QueryMultiOrderAndGroupTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>().TakePage(1, 10).OrderByDescending(f => f.CreateTime).OrderBy(f => f.SerialNo).GroupBy(f => new { f.CreateTime, f.SerialNo });
            IList<OrderModel> result = await query.Select(f => new { f.CreateTime, f.SerialNo, Count = Function.Count(), Max = Function.Max(f.Amount) }).ToListAsync<OrderModel>();
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public void SelectObjectTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<OrderAliasModel> models = query.Where(f => f.Remark.Contains("ABCD")).Select(f => new
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
            Assert.IsTrue(models.Any(f => f.C.Contains("ABCD")));
        }

        [TestMethod]
        public void SelectParameterObjectTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<OrderAliasModel> models = query.Where(f => f.Remark.Contains("ABCD")).Select(f => new { f }).ToList<OrderAliasModel>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any(f => f.C.Contains("ABCD")));
        }

        [TestMethod]
        public void SelectAliasEntityTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<OrderAliasModel> models = query.Where(f => f.Remark.Contains("ABCD")).Select(f => new OrderAliasModel
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
            Assert.IsTrue(models.Any(f => f.C.Contains("ABCD")));
        }


        [TestMethod]
        public void SelectEntitySwapPropertyTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> models = query.Where(f => f.Remark.Contains("ABCD")).Select(f => new Order
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
            }).ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any(f => f.SerialNo.Contains("ABCD")));
        }

        [TestMethod]
        public void SelectEntityTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<OrderModel> models = query.Where(f => f.Remark.Contains("ACD")).Select(f => new Order { Id = f.Id, Remark = f.Remark, Ignore = f.Ignore, SerialNo = f.SerialNo, IsDelete = f.IsDelete }).ToList<OrderModel>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any(f => f.Remark.Contains("ACD")));
        }


        [TestMethod]
        public void SelectNullableGuidTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Guid?> models = query.Where(f => f.Remark.Contains("ACD")).Select(f => f.DocId).ToList<Guid?>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any(f => !f.HasValue));
        }

        [TestMethod]
        public void SelectDistinctNameTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<string> models = query.Where(f => f.Status == Status.Running).Select(f => f.SerialNo).Distinct().ToList<string>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Distinct().Count() == models.Count);
        }

        [TestMethod]
        public void ColumnAliasTest()
        {
            using IDbConnection connection = CreateConnection();
            IList<Order> orders = connection.Query<Order>().TakePage(1, 20).ToList<Order>();
            Assert.IsNotNull(orders);
            Assert.IsTrue(orders.All(f => !string.IsNullOrEmpty(f.SerialNo)));
        }

        [TestMethod]
        public void ColumnAliasSelectObjectTest()
        {
            using IDbConnection connection = CreateConnection();
            IList<Order> orders = connection.Query<Order>().Select(f => new { f.SerialNo }).TakePage(1, 20).ToList<Order>();
            Assert.IsNotNull(orders);
            Assert.IsTrue(orders.All(f => !string.IsNullOrEmpty(f.SerialNo)));
        }


        [TestMethod]
        public void ColumnAliasSelectEntityTest()
        {
            using IDbConnection connection = CreateConnection();
            IList<Order> orders = connection.Query<Order>().Select(f => new Order { SerialNo = f.SerialNo }).TakePage(1, 20).ToList<Order>();
            Assert.IsNotNull(orders);
            Assert.IsTrue(orders.All(f => !string.IsNullOrEmpty(f.SerialNo)));
        }

        [TestMethod]
        public void EqualsTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<string> models = query.Where(f => f.SerialNo.Equals("CA020C42")).Select(f => f.SerialNo).Distinct().ToList<string>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.All(f => f == "CA020C42"));
        }

        [TestMethod]
        public void GreaterThanTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> models = query.Where(f => f.Version > 10).ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }


        [TestMethod]
        public void ExpressionOrderByTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> models = query.Where(f => f.Version > 10).OrderBy(f => new { f.SerialNo, f.CreateTime }).OrderBy(f => f.IsDelete).OrderBy(f => new Order { IsActive = f.IsActive }).ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void ExpressionOrderByDescendingTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> models = query.Where(f => f.Version > 10).OrderBy(f => new { f.SerialNo, f.CreateTime }).OrderByDescending(f => f.IsActive).OrderByDescending(f => new Order { IsDelete = f.IsDelete }).ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void GroupByTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> models = query.Where(f => f.Version > 10)
                .OrderBy(f => new { f.CreateTime })
                .GroupBy(f => f.SerialNo)
                .GroupBy(f => new { f.IsDelete })
                .GroupBy(f => new Order { SerialNo = f.SerialNo })
                .ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void HavingTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> models = query.Where(f => f.Version > 10)
                .GroupBy(f => new { f.SerialNo, f.Version })
                .Having(f => f.Version > 30)
                .Select(f => new { f.Version, f.SerialNo, Total = Function.Count() })
                .ToList<Order>();

            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void WhereIsNullTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> models = query.Where(f => f.DocId == null).ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void WhereIsNotNullTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> models = query.Where(f => f.DocId != null).ToList<Order>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void MaxStringTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            string max = query.Where(f => f.DocId != null).Max(f => f.SerialNo);
            Assert.IsNotNull(max);
            Console.WriteLine(max);
        }

        [TestMethod]
        public void MaxIntTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            int max = query.Where(f => f.DocId != null).Max(f => f.Version);
            Assert.IsTrue(max > 0);
            Console.WriteLine(max);
        }


        [TestMethod]
        public void MaxNullableTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            int? max = query.Where(f => f.Version < 0).Max(f => (int?)f.Version);
            Assert.IsFalse(max.HasValue);
        }

        [TestMethod]
        public void MaxStringNullableTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            string max = query.Where(f => f.Version < 0).Max(f => f.SerialNo);
            Assert.IsNull(max);
        }

        [TestMethod]
        public async Task MaxNullableAsyncTest()
        {

            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            DateTime? max = await query.Where(f => f.DocId != null).MaxAsync(f => f.UpdateTime);
            Assert.IsFalse(max.HasValue);
        }

        [TestMethod]
        public async Task MaxNullableNoDataAsyncTest()
        {

            using IDbConnection connection = CreateConnection();
            Query<Emit> query = connection.Query<Emit>();
            int? max = await query.Where(f => f.Version > 10).MaxAsync(f => (int?)f.Version);
            Assert.IsFalse(max > 0);
        }


        [TestMethod]
        public void MinIntTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            int min = query.Where(f => !f.IsDelete).Min(f => f.Version);
            Assert.IsTrue(min > 0);
        }

        [TestMethod]
        public void MinNullableTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            DateTime? max = query.Where(f => f.DocId != null).Min(f => f.UpdateTime);
            Assert.IsFalse(max.HasValue);
        }

        [TestMethod]
        public void MinNullableHasValueTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            DateTime? max = query.Where(f => f.DocId.HasValue).Min(f => f.UpdateTime);
            Assert.IsFalse(max.HasValue);
        }

        [TestMethod]
        public async Task MinNullableAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            DateTime? max = await query.Where(f => f.DocId != null).MinAsync(f => f.UpdateTime);
            Assert.IsFalse(max.HasValue);
        }


        [TestMethod]
        public void SumIntTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            decimal? max = query.Where(f => f.Status == Status.Running).Sum(f => f.Freight);
            Assert.IsTrue(max.HasValue);
        }


        [TestMethod]
        public void SumNullableTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            int? max = query.Where(f => f.Version < 0).Sum(f => (int?)f.Version);
            Assert.IsFalse(max > 0);
        }

        [TestMethod]
        public async Task SumAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            int max = await query.Where(f => f.DocId != null).SumAsync(f => f.Version);
            Assert.IsTrue(max > 0);
        }

        [TestMethod]
        public void NotSupportTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            Assert.ThrowsException<NotSupportedException>(() => query.Where(f => f.DocId != null).Sum(f => f.UpdateTime));
        }

        [TestMethod]
        public void DateAddYearsTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> entities = query.Where(f => f.CreateTime > DateTime.Now.AddYears(-2).AddYears(1)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void DateAddMonthsTest()
        {

            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> entities = query.Where(f => f.CreateTime > DateTime.Now.AddMonths(-2)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void DateAddDaysTest()
        {

            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> entities = query.Where(f => f.CreateTime > DateTime.Now.AddDays(-20)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void DateAddHoursTest()
        {

            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> entities = query.Where(f => f.CreateTime > DateTime.Now.Date.AddHours(-500)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void DateAddMinutesTest()
        {

            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            DateTime date = DateTime.Now;
            IList<Order> entities = query.Where(f => f.CreateTime > date.Date.AddMinutes(-500 * 60)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void DateAddSecondsTest()
        {

            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            DateTime date = DateTime.Now;
            IList<Order> entities = query.Where(f => f.CreateTime > date.Date.AddSeconds(-500 * 60 * 60)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void DateAddMillisecondsTest()
        {

            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            DateTime date = DateTime.Now;
            IList<Order> entities = query.Where(f => f.CreateTime > date.Date.AddMilliseconds(-500 * 60 * 60 * 1000)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void NullableParamTest()
        {

            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            DateTime? date = new DateTime(2021, 3, 10);
            IList<Order> entities = query.Where(f => f.CreateTime > date.Value).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void NullableDateTimeValueDateTest()
        {

            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            DateTime? date = DateTime.Now.AddDays(-20);
            IList<Order> entities = query.Where(f => f.CreateTime > date.Value.Date).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void NotBoolHasValueTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> entities = query.Where(f => f.DocId.HasValue).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void BoolNotHasValueTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> entities = query.Where(f => !f.DocId.HasValue).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void BoolHasValueTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> entities = query.Where(f => f.IsActive.Value).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void NotBoolNotHasValueTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> entities = query.Where(f => !f.IsActive.Value).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void BoolValueTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Status> testTypes = new List<Status> { Status.Running };
            IList<Order> entities = query.Where(f => testTypes.Contains(f.Status) && f.DocId.HasValue && !f.IsDelete && f.IsActive.Value).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }


        [TestMethod]
        public void EnumListContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Status> testTypes = new List<Status> { Status.Running };
            IList<Order> entities = query.Where(f => testTypes.Contains(f.Status)).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void ToStringTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Status> testTypes = new List<Status> { Status.Running };
            IList<Order> entities = query.Where(f => testTypes.Contains(f.Status)).Select(f => new { f.Id, Name = (f.Freight ?? 0).ToString(CultureInfo.InvariantCulture), f.SerialNo }).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void NullableBoolEqualsTrueTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> entities = query.Where(f => f.IsActive == true).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void NullableBoolEqualsFalseTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> entities = query.Where(f => f.IsActive == false).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void NullableNumberEqualsTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> entities = query.Where(f => f.Freight > 1.60479319m).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void NullableNumberConvertCompareTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> entities = query.Where(f => f.Freight > 1).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void NullableNumberCompareTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> entities = query.Where(f => f.Freight > 1m).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void NullableEnumEqualTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> entities = query.Where(f => f.Status == Status.Running).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void NullableEnumCompareTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> entities = query.Where(f => f.Status >= Status.Running).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void ExistTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            IList<Order> entities = query.Where(f => f.Status >= Status.Running).Exist<Item>((t, b) => t.Id == b.OrderId && b.Index > 5).ToList<Order>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void PageQueryTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>().Where(f => f.Status >= Status.Running)
                .Exist<Item>((t, b) => t.Id == b.OrderId && b.Index > 5)
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
            Query<Order> query = connection.Query<Order>().Where(f => f.Status >= Status.Running).Between(f => f.SerialNo, "123", "456")
                .TakePage(1, 10);
            int count = query.Count();
            Assert.IsTrue(count > 0);
            IList<Order> orders = query.ToList<Order>();
            Assert.IsTrue(orders.Any());
        }

        [TestMethod]
        public void SegregateTest()
        {
            using IDbConnection connection = CreateConnection();
            string sql = connection.Query<Order>().Where(f => f.Status >= Status.Running)
                  .Where(f => f.SerialNo.Contains("abc") || f.Remark.Contains("abc"))
                  .Select(f => f.Id)
                  .GetCommandText();
            string expectSql =
                "SELECT `t1`.`Id` from `order` AS `t1` WHERE `t1`.`Status`>=@w_p_1 AND (`t1`.`Number` LIKE @w_p_2 OR `t1`.`Remark` LIKE @w_p_3)";
            Assert.IsTrue(string.Compare(TrimAllEmpty(sql), TrimAllEmpty(expectSql), StringComparison.OrdinalIgnoreCase) == 0);
        }

        [TestMethod]
        public void Segregate1Test()
        {
            using IDbConnection connection = CreateConnection();
            string sql = connection.Query<Order>().Where(f => f.Status == Status.Running || f.IsDelete)
                .Where(f => f.SerialNo.Contains("abc") || f.Remark.Contains("abc"))
                .Select(f => f.Id)
                .GetCommandText();
            string expectSql =
                "SELECT `t1`.`Id` from `order` AS `t1` WHERE (`t1`.`Status`=@w_p_1 OR `t1`.`IsDelete`=@w_p_2) AND (`t1`.`Number` LIKE @w_p_3 OR `t1`.`Remark` LIKE @w_p_4)";
            Debug.WriteLine(sql);
            Debug.WriteLine(expectSql);
            Assert.IsTrue(string.Compare(TrimAllEmpty(sql), TrimAllEmpty(expectSql), StringComparison.OrdinalIgnoreCase) == 0);
        }

        [TestMethod]
        public void Segregate2Test()
        {
            using IDbConnection connection = CreateConnection();
            string sql = connection.Query<Order>().Where(f => (f.Status == Status.Running || f.IsDelete) && f.IsActive == true)
                .Where(f => f.SerialNo.Contains("abc") || f.Remark.Contains("abc"))
                .Select(f => f.Id)
                .GetCommandText();
            string expectSql =
                "SELECT `t1`.`Id` from `order` AS `t1` WHERE ((`t1`.`Status`=@w_p_1 OR `t1`.`IsDelete`=@w_p_2) AND `t1`.`IsActive`=@w_p_3) AND (`t1`.`Number` LIKE @w_p_4 OR `t1`.`Remark` LIKE @w_p_5)";
            Debug.WriteLine(sql);
            Debug.WriteLine(expectSql);
            Assert.IsTrue(string.Compare(TrimAllEmpty(sql), TrimAllEmpty(expectSql), StringComparison.OrdinalIgnoreCase) == 0);
        }

        [TestMethod]
        public void Segregate3Test()
        {
            using IDbConnection connection = CreateConnection();
            string sql = connection.Query<Order>().Where(f => f.Status == Status.Running || f.IsDelete && (f.SerialNo.Contains("abc") || f.Remark.Contains("abc")))
                .Select(f => f.Id)
                .GetCommandText();
            var expectSql = "SELECT `t1`.`Id` from `order` AS `t1` WHERE (`t1`.`Status`=@w_p_1 OR (`t1`.`IsDelete`=@w_p_2 AND (`t1`.`Number` LIKE @w_p_3 OR `t1`.`Remark` LIKE @w_p_4)))";
            Debug.WriteLine(sql);
            Debug.WriteLine(expectSql);
            Assert.IsTrue(string.Compare(TrimAllEmpty(sql), TrimAllEmpty(expectSql), StringComparison.OrdinalIgnoreCase) == 0);
        }

        [TestMethod]
        public void Segregate4Test()
        {
            using IDbConnection connection = CreateConnection();
            string sql = connection.Query<Order>().Where(f => (f.Status == Status.Running || f.IsDelete) && f.CreateTime > DateTime.Now || f.SerialNo.Contains("abc") || f.Remark.Contains("abc"))
                .Select(f => f.Id)
                .GetCommandText();
            string expectSql =
                "SELECT `t1`.`Id` from `order` AS `t1` WHERE (((`t1`.`Status`=@w_p_1 OR `t1`.`IsDelete`=@w_p_2) AND `t1`.`CreateTime`> NOW()) OR `t1`.`Number` LIKE @w_p_3 OR `t1`.`Remark` LIKE @w_p_4)";
            Debug.WriteLine(sql);
            Debug.WriteLine(expectSql);
            Assert.IsTrue(string.Compare(TrimAllEmpty(sql), TrimAllEmpty(expectSql), StringComparison.OrdinalIgnoreCase) == 0);
        }

        [TestMethod]
        public void UpdateAssignTest()
        {
            using IDbConnection connection = CreateConnection();
            Guid buyerId = Guid.Parse("0175eeeb-eefe-4e5b-bd20-e99d6be2f964");
            string number = "123455";
            Status status = Status.Running;
            SignState? signState = SignState.Signed;
            decimal amount = 124.66m;
            Guid docId = Guid.Parse("010a7ef7-8802-47b3-a134-7742dce598d0");
            bool isDelete = false;
            bool isActive = true;
            DateTime time = DateTime.Now;
            int version = 100;
            var result = connection.Update<Order>(f => f.Id == Guid.Parse("009846a5-f96d-4583-bbac-d8c7056c1d2a"), f => new Order
            {
                BuyerId = buyerId,
                SerialNo = number,
                Remark = "test",
                Status = status,
                SignState = signState,
                Amount = amount,
                Freight = 123.456m,
                DocId = docId,
                IsDelete = isDelete,
                IsActive = isActive,
                CreateTime = time,
                UpdateTime = time,
                Version = version
            });
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void DateTimeParseTest()
        {
            using IDbConnection connection = CreateConnection();
            var result = connection.Update<Order>(f => f.Id == Guid.Parse("009846a5-f96d-4583-bbac-d8c7056c1d2a"), f => new Order
            {
                UpdateTime = DateTime.Parse("2021-04-02 15:56:20")
            });
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void EnumerableEmptyTest()
        {
            using IDbConnection connection = CreateConnection();
            Guid id = Guid.Parse("009846a5-f96d-4583-bbac-d8c7056c1d2a");
            IEnumerable<Guid> docIds = Enumerable.Empty<Guid>();
            var result = connection.Update<Order>(f => docIds.Contains(f.Id) && f.Id == id, f => new Order
            {
                UpdateTime = DateTime.Parse("2021-04-02 15:56:20")
            });
            Assert.AreEqual(result, 0);
        }

        [TestMethod]
        public void ListEmptyTest()
        {
            using IDbConnection connection = CreateConnection();
            Guid id = Guid.Parse("009846a5-f96d-4583-bbac-d8c7056c1d2a");
            IList<Guid> docIds = Enumerable.Empty<Guid>().ToList();
            var result = connection.Update<Order>(f => docIds.Contains(f.Id) && f.Id == id, f => new Order
            {
                UpdateTime = DateTime.Parse("2021-04-02 15:56:20")
            });
            Assert.AreEqual(result, 0);
        }

        /// <summary>
        /// whereif测试
        /// </summary>
        [TestMethod]
        public void WhereIfEmptyParamTest()
        {
            QueryParam queryParam = new QueryParam();
            using IDbConnection connection = CreateConnection();
            IQuery query = connection.Query<Order>()
                  .WhereIf(queryParam.CreateTime.HasValue, f => f.CreateTime > queryParam.CreateTime)
                  .WhereIf(queryParam.IsDelete == true, f => f.IsDelete)
                  .WhereIf(!string.IsNullOrWhiteSpace(queryParam.Key), f => f.Remark.Contains(queryParam.Key))
                  .Select(g => g.Id);
            string commandText = query.GetCommandText();
            Assert.AreEqual("select `t1`.`Id` from `order` AS `t1`", commandText, true);
        }

        /// <summary>
        /// whereif测试
        /// </summary>
        [TestMethod]
        public void WhereIfFullParamTest()
        {
            QueryParam queryParam = new QueryParam { CreateTime = DateTime.Now.AddDays(-10), IsDelete = true, Key = "1234" };
            using IDbConnection connection = CreateConnection();
            IQuery query = connection.Query<Order>()
                  .WhereIf(queryParam.CreateTime.HasValue, f => f.CreateTime > queryParam.CreateTime)
                  .WhereIf(queryParam.IsDelete == true, f => f.IsDelete)
                  .WhereIf(!string.IsNullOrWhiteSpace(queryParam.Key), f => f.Remark.Contains(queryParam.Key))
                  .Select(g => g.Id);
            string commandText = query.GetCommandText();
            Assert.AreEqual("SELECT `t1`.`Id` FROM `order` AS `t1` WHERE `t1`.`CreateTime` > @w_p_1 AND `t1`.`IsDelete` = @w_p_2 AND `t1`.`Remark` LIKE @w_p_3", commandText, true);
        }

        /// <summary>
        /// whereif测试
        /// </summary>
        [TestMethod]
        public void WhereIfPartialParamTest()
        {
            QueryParam queryParam = new QueryParam { CreateTime = DateTime.Now.AddDays(-10), IsDelete = true };
            using IDbConnection connection = CreateConnection();
            IQuery query = connection.Query<Order>()
                  .WhereIf(queryParam.CreateTime.HasValue, f => f.CreateTime > queryParam.CreateTime)
                  .WhereIf(queryParam.IsDelete == true, f => f.IsDelete)
                  .WhereIf(!string.IsNullOrWhiteSpace(queryParam.Key), f => f.Remark.Contains(queryParam.Key))
                  .Select(g => g.Id);
            string commandText = query.GetCommandText();
            Assert.AreEqual("SELECT `t1`.`Id` FROM `order` AS `t1` WHERE `t1`.`CreateTime` > @w_p_1 AND `t1`.`IsDelete` = @w_p_2", commandText, true);
        }

        /// <summary>
        /// 按常量排序
        /// </summary>
        [TestMethod]
        public void OrderByConstTest()
        {
            using IDbConnection connection = CreateConnection();
            QueryParam queryParam = new QueryParam { Key = "CreateTime" };
            Query<Order> query = connection.Query<Order>();
            query.Where((f) => !f.IsDelete)
                  .OrderBy(queryParam.Key)
                  .OrderBy("Amount")
                  .OrderBy((f) => f.Index);
            Order order = query.FirstOrDefault<Order>();
            Assert.IsNotNull(order);
            //ORDER BY `t1`.`CreateTime` ASC ,`t1`.`Amount` ASC ,`t1`.`Index` ASC  
        }

        /// <summary>
        /// 并发查询测试
        /// </summary>
        [TestMethod]
        public async Task ConcurrentQueryTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<Order> query = connection.Query<Order>();
            query.Where(v => v.Remark.Contains("FD2"));
            bool data = await query.AnyAsync();
            Assert.IsTrue(data);
            List<Task> tasks1 = Enumerable.Range(0, 10).Select((_) => CreateStringTasks()).ToList();
            List<Task> tasks2 = Enumerable.Range(0, 10).Select((_) => CreateGuidTasks()).ToList();
            List<Task> tasks3 = Enumerable.Range(0, 10).Select((_) => CreateDateTimeTasks()).ToList();
            List<Task> tasks4 = Enumerable.Range(0, 10).Select((_) => CreateAddDaysTasks()).ToList();
            await Task.WhenAll(tasks1.Concat(tasks2).Concat(tasks3).Concat(tasks4));
        }

        private Task CreateStringTasks()
        {
            using IDbConnection connection = CreateConnection();
            return Task.Run(() => connection.Query<Order>().Where(x => new List<string> { "abcd" }.Contains(x.SerialNo)).GetCommandText());
        }

        private Task CreateGuidTasks()
        {
            using IDbConnection connection = CreateConnection();
            connection.Open();
            return Task.Run(() => connection.Query<Order>().Where(x => new List<Guid> { Guid.NewGuid() }.Contains(x.Id)).GetCommandText());
        }

        private Task CreateDateTimeTasks()
        {
            using IDbConnection connection = CreateConnection();
            return Task.Run(() => connection.Query<Order>().Where(x => new List<DateTime> { DateTime.Now }.Contains(x.CreateTime)).GetCommandText());
        }

        private Task CreateAddDaysTasks()
        {
            using IDbConnection connection = CreateConnection();
            return Task.Run(() => connection.Query<Order>().Where(x => x.CreateTime.AddDays(1) > DateTime.Now).GetCommandText());
        }

        private static string TrimAllEmpty(string content)
        {
            return string.IsNullOrWhiteSpace(content) ? content : content.Replace(" ", "");
        }
    }
}