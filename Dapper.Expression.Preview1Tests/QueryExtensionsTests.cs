using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Serialization;
using IsolationLevel = System.Data.IsolationLevel;


namespace Dapper.Extensions.Expression.UnitTests
{
    [TestClass]
    public class QueryExtensionsTests
    {
        private static IDbConnection CreateConnection()
        {
            IDbConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8");
            return connection;
        }

        #region 写入测试

        [TestMethod]
        public void InsertBulkTest()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            IList<TestEntity> entities1 = Enumerable.Range(0, 500).Select((f, index) => new TestEntity
            {
                Id = Guid.NewGuid(),
                RequestId = Guid.NewGuid(),
                IsDelete = false,
                Logged = DateTime.Now,
                Name = GetRandomString(5, 9),
                Code = GetRandomString(5, 9),
                Number = GetRandomString(5, 9),
                UpdateTime = DateTime.Now,
                Type = (TestType)(index % 2),
                Message = GetRandomString(10, 40),
                Version = index
            }).ToList();
            using IDbConnection connection = CreateConnection();
            int value1 = connection.InsertBulk(entities1);
            stopwatch.Stop();
            Console.WriteLine("InsertBulk写入value1={0}条,耗时{1}", value1, stopwatch.Elapsed);
            Assert.AreEqual(value1, entities1.Count);
        }

        [TestMethod]
        public void InsertBulkPartFailTest()
        {
            IList<TestEntity> entities1 = Enumerable.Range(0, 50).Select((f, index) => new TestEntity
            {
                Id = Guid.NewGuid(),
                IsDelete = false,
                Logged = DateTime.Now,
                Name = GetRandomString(index * 10 + 1, 60)
            }).ToList();
            Assert.ThrowsException<MySqlException>(() =>
            {
                using IDbConnection connection = CreateConnection();
                return connection.InsertBulk(entities1);
            });
        }

        [TestMethod]
        public void InsertBulkPartFailWithDbTransactionTest()
        {
            using IDbConnection connection = CreateConnection();
            connection.Open();
            IDbTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                IList<TestEntity> entities1 = Enumerable.Range(0, 50).Select((f, index) => new TestEntity
                {
                    Id = Guid.NewGuid(),
                    IsDelete = false,
                    Logged = DateTime.Now,
                    Name = GetRandomString(index * 10 + 1, 60)
                }).ToList();
                int value1 = connection.InsertBulk(entities1);
                transaction.Commit();
                stopwatch.Stop();
                Assert.IsTrue(value1 > 0);
                Assert.AreNotEqual(value1, entities1.Count);
            }
            catch
            {
                transaction.Rollback();
            }
        }

        [TestMethod]
        public void InsertBulkPartFailWithTransactionScopeTest()
        {
            IList<TestEntity> entities1 = Enumerable.Range(0, 50).Select((f, index) => new TestEntity
            {
                Id = Guid.NewGuid(),
                IsDelete = false,
                Logged = DateTime.Now,
                Name = GetRandomString(index * 10 + 1, 60)
            }).ToList();
            Assert.ThrowsException<MySqlException>(() =>
            {
                using IDbConnection connection = CreateConnection();
                using var trans = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted });
                connection.InsertBulk(entities1);
                trans.Complete();
            });
        }

        /// <summary>
        /// 写入单条测试
        /// </summary>

        [TestMethod]
        public void InsertOneTest()
        {
            using IDbConnection connection = CreateConnection();
            TestEntity insertEntity = new TestEntity { Id = Guid.NewGuid(), IsDelete = false, Logged = DateTime.Now, Name = nameof(Program) };
            int value = connection.Insert(insertEntity);
            Assert.AreEqual(1, value);
        }

        /// <summary>
        /// 写入多条测试
        /// </summary>
        [TestMethod]
        public void InsertManyTest()
        {
            using IDbConnection connection = CreateConnection();
            IList<TestEntity> entities = Enumerable.Range(0, 10).Select(f => new TestEntity { Id = Guid.NewGuid(), IsDelete = false, Logged = DateTime.Now, Name = nameof(Program) }).ToList();
            int values = connection.Insert(entities);
            Assert.AreEqual(values, entities.Count);
        }

        /// <summary>
        /// 写入多条部分失败测试
        /// </summary>
        [TestMethod]
        public void InsertManyAndInPartFailTest()
        {
            IList<TestEntity> entities = Enumerable.Range(0, 50).Select((f, idx) => new TestEntity
            {
                Id = Guid.NewGuid(),
                IsDelete = false,
                Logged = DateTime.Now,
                Name = GetRandomString(idx * 10 + 1, 60)
            }).ToList();
            Assert.ThrowsException<MySqlException>(() =>
            {
                using IDbConnection connection = CreateConnection();
                return connection.Insert(entities);
            });
        }

        /// <summary>
        /// 事务写入多条部分失败测试
        /// </summary>
        [TestMethod]
        public void InsertManyAndInPartFailWithTransactionTest()
        {
            using IDbConnection connection = CreateConnection();
            connection.Open();
            IDbTransaction transaction = connection.BeginTransaction();
            IList<TestEntity> entities = Enumerable.Range(0, 10).Select((f, idx) => new TestEntity
            {
                Id = Guid.NewGuid(),
                IsDelete = false,
                Logged = DateTime.Now,
                Name = GetRandomString(10 * idx + 1, 100)
            }).ToList();
            int values = 0;
            try
            {
                values = connection.Insert(entities, transaction);
                transaction.Commit();
                Assert.IsTrue(values > 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                transaction.Rollback();
                Assert.AreEqual(0, values);
            }
        }

        private static string GetRandomString(int length, int max)
        {
            int len = length % max + 1;
            byte[] bytes = new byte[len];
            Span<byte> span = new Span<byte>(bytes, 0, bytes.Length);
            RandomNumberGenerator.Fill(span);
            StringBuilder builder = new StringBuilder();
            foreach (byte b in span)
            {
                builder.AppendFormat("{0:X2}", b);
            }
            return builder.ToString();
        }

        /// <summary>
        /// 事务写入多条部分失败测试
        /// </summary>
        [TestMethod]
        public void InsertManyAndInPartFailWithTransactionScopeTest()
        {
            IList<TestEntity> entities = Enumerable.Range(0, 100).Select((f, idx) => new TestEntity
            {
                Id = Guid.NewGuid(),
                IsDelete = false,
                Logged = DateTime.Now,
                Name = GetRandomString(idx, 60)
            }).ToList();
            Assert.ThrowsException<MySqlException>(() =>
            {
                using IDbConnection connection = CreateConnection();
                using TransactionScope trans = new TransactionScope();
                connection.Insert(entities);
                trans.Complete();
            });
        }

        /// <summary>
        /// 事务写入多条部分失败测试
        /// </summary>
        [TestMethod]
        public void InsertManyWithTransactionTest()
        {
            using IDbConnection connection = CreateConnection();
            connection.Open();
            IDbTransaction transaction = connection.BeginTransaction();
            try
            {
                IList<TestEntity> entities = Enumerable.Range(0, 10).Select((f, idx) => new TestEntity
                {
                    Id = Guid.NewGuid(),
                    IsDelete = false,
                    Logged = DateTime.Now,
                    Name = GetRandomString(10, 10)
                }).ToList();
                int values = connection.Insert(entities, transaction);
                transaction.Commit();
                Assert.IsTrue(values > 0);
                Assert.AreEqual(values, entities.Count);
            }
            catch
            {
                transaction.Rollback();
            }
        }

        /// <summary>
        /// 事务写入多条部分失败测试
        /// </summary>
        [TestMethod]
        public void InsertManyWithTransactionScopeTest()
        {
            using IDbConnection connection = CreateConnection();
            using TransactionScope trans = new TransactionScope();
            IList<TestEntity> entities = Enumerable.Range(0, 10).Select((f, idx) => new TestEntity
            {
                Id = Guid.NewGuid(),
                IsDelete = false,
                Logged = DateTime.Now,
                Name = GetRandomString(idx + 1, 60)
            }).ToList();
            int values = connection.Insert(entities);
            trans.Complete();
            Assert.IsTrue(values > 0);
            Assert.AreEqual(values, entities.Count);
        }

        /// <summary>
        /// 异步写入测试
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task InsertOneAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            TestEntity testEntity = new TestEntity
            {
                Id = Guid.NewGuid(),
                IsDelete = false,
                Logged = DateTime.Now,
                Name = GetRandomString(5, 9),
                Code = GetRandomString(5, 9),
                Number = GetRandomString(5, 9),
                UpdateTime = DateTime.Now,
                Type = TestType.Trace,
                Message = GetRandomString(10, 40)
            };
            int result = await connection.InsertAsync(testEntity);
            Assert.IsTrue(result > 0);
        }


        /// <summary>
        /// 异步写入多条测试
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task InsertManyAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            IEnumerable<TestEntity> entities = Enumerable.Range(0, 100).Select(f => new TestEntity
            {
                Id = Guid.NewGuid(),
                IsDelete = false,
                Logged = DateTime.Now,
                Name = GetRandomString(5, 9),
                Code = GetRandomString(5, 9),
                Number = GetRandomString(5, 9),
                UpdateTime = DateTime.Now,
                Type = TestType.Trace,
                Message = GetRandomString(10, 40)
            });
            int result = await connection.InsertAsync(entities);
            Assert.IsTrue(result > 0);
        }

        /// <summary>
        /// 异步批量写入测试
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task InsertBulkAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            IList<TestEntity> entities = Enumerable.Range(0, 100).Select(f => new TestEntity
            {
                Id = Guid.NewGuid(),
                IsDelete = false,
                Logged = DateTime.Now,
                Name = GetRandomString(5, 9),
                Code = GetRandomString(5, 9),
                Number = GetRandomString(5, 9),
                UpdateTime = DateTime.Now,
                Type = TestType.Trace,
                Message = GetRandomString(10, 40)
            }).ToList();
            int result = await connection.InsertBulkAsync(entities);
            Assert.IsTrue(result > 0);
        }

        #endregion


        #region 更新测试


        /// <summary>
        /// 获取表名测试
        /// </summary>
        [TestMethod]
        public void GetTableNameTest()
        {
            using IDbConnection connection = CreateConnection();
            string tableName = connection.GetTableName<TestEntity>();
            Assert.AreEqual("`testentity`", tableName);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public void UpdateTest()
        {
            TestEntity testEntity = new TestEntity { Id = new Guid("0029adf0-8863-4905-8abe-6e66ad314556"), Name = GetType().Name, Logged = DateTime.Now };
            using IDbConnection connection = CreateConnection();
            int updated = connection.Update(testEntity);
            Assert.IsTrue(updated > 0);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public async Task UpdateAsyncTest()
        {
            TestEntity testEntity = new TestEntity
            {
                Id = new Guid("0029adf0-8863-4905-8abe-6e66ad314556"),
                Name = GetType().Name,
                Logged = DateTime.Now,
                Message = GetRandomString(10, 20),
                Type = TestType.Trace
            };
            using IDbConnection connection = CreateConnection();
            int updated = await connection.UpdateAsync(testEntity);
            Assert.IsTrue(updated > 0);
        }


        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public void UpdateObjectTest()
        {
            string id = "00007165-2d05-4683-b064-5fbd2d332885";
            using IDbConnection connection = CreateConnection();
            int updated = connection.Update<TestEntity>(f => f.Id == Guid.Parse(id), f => new { Logged = DateTime.Now, IsDelete = true, Name = $"'{GetType().Name}'" });
            Assert.IsTrue(updated > 0);
        }


        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public async Task UpdateObjectAsyncTest()
        {
            string id = "00007165-2d05-4683-b064-5fbd2d332885";
            using IDbConnection connection = CreateConnection();
            int updated = await connection.UpdateAsync<TestEntity>(f => f.Id == Guid.Parse(id), f => new
            {
                Logged = DateTime.Now,
                IsDelete = true,
                Message = GetRandomString(20, 30)
            });
            Assert.IsTrue(updated > 0);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public void UpdateEntityTest()
        {
            using IDbConnection connection = CreateConnection();
            string id = "002b9904-e84b-48ee-ac60-b2e948b520ca";
            int updated = connection.Update<TestEntity>(f => f.Id == Guid.Parse(id), f => new TestEntity { Logged = DateTime.Now, IsDelete = true, Name = $"'{GetType().Name}'" });
            Assert.IsTrue(updated > 0);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public async Task UpdateEntityAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            string id = "002b9904-e84b-48ee-ac60-b2e948b520ca";
            int updated = await connection.UpdateAsync<TestEntity>(f => f.Id == Guid.Parse(id), f => new TestEntity
            {
                Logged = DateTime.Now,
                IsDelete = true,
                Name = $"'{GetType().Name}'",
                Message = GetRandomString(10, 20)
            });
            Assert.IsTrue(updated > 0);
        }

        #endregion

        #region 删除测试

        /// <summary>
        /// 删除测试
        /// </summary>
        [TestMethod]
        public void DeleteTest()
        {
            using IDbConnection connection = CreateConnection();
            TestEntity insertEntity = new TestEntity { Id = Guid.NewGuid(), IsDelete = false, Logged = DateTime.Now, Name = nameof(Program) };
            int value = connection.Insert(insertEntity);
            Assert.AreEqual(1, value);
            int deleted = connection.Delete(insertEntity);
            Assert.IsTrue(deleted > 0);
        }

        /// <summary>
        /// 删除测试
        /// </summary>
        [TestMethod]
        public async Task DeleteAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            TestEntity testEntity = new TestEntity
            {
                Id = Guid.NewGuid(),
                IsDelete = false,
                Logged = DateTime.Now,
                Name = GetRandomString(5, 9),
                Code = GetRandomString(5, 9),
                Number = GetRandomString(5, 9),
                UpdateTime = DateTime.Now,
                Type = TestType.Trace,
                Message = GetRandomString(10, 40)
            };
            int deleted = await connection.DeleteAsync(testEntity);
            Assert.IsFalse(deleted > 0);
            int result = await connection.InsertAsync(testEntity);
            Assert.IsTrue(result > 0);
            deleted = await connection.DeleteAsync(testEntity);
            Assert.IsTrue(deleted > 0);
        }

        /// <summary>
        /// 删除测试
        /// </summary>
        [TestMethod]
        public void DeleteAllTest()
        {
            using IDbConnection connection = CreateConnection();
            int deleted = connection.DeleteAll<TestEntityBak>();
            Assert.IsTrue(deleted > 0);
        }

        /// <summary>
        /// 删除测试
        /// </summary>
        [TestMethod]
        public async Task DeleteAllAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            int deleted = await connection.DeleteAllAsync<TestEntityBak>();
            Assert.IsTrue(deleted > 0);
        }

        /// <summary>
        /// 删除测试
        /// </summary>
        [TestMethod]
        public void DeleteByExpressionTest()
        {
            using IDbConnection connection = CreateConnection();
            int deleted = connection.Delete<TestEntity>(f => f.Name.Contains("efg") || f.Number.Contains("afd"));
            Assert.IsTrue(deleted > 0);
        }

        /// <summary>
        /// 删除测试
        /// </summary>
        [TestMethod]
        public async Task DeleteByExpressionAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            int deleted = await connection.DeleteAsync<TestEntity>(f => f.Name.Contains("74F") || f.Number.Contains("afd"));
            Assert.IsTrue(deleted > 0);
        }

        #endregion

        #region 查询测试

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void QueryContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            query.Where(v => v.Name.Contains("FD2"));
            IEnumerable<TestEntity> data = query.ToList<TestEntity>();
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
                Query<TestEntity> query = new Query<TestEntity>(connection);
                query.Where(v => v.Name.Contains("FD2"));
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
            Query<TestEntity> query = new Query<TestEntity>(connection);
            query.Where(v => v.Name.Contains("FD2"));
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
                Query<TestEntity> query = new Query<TestEntity>(connection);
                query.Where(v => v.Name.Contains("FD2"));
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
                    Query<TestEntity> query = new Query<TestEntity>(connection);
                    query = query.Where(v => v.Name.Contains("FD2"));
                    IList<TestEntity> data = query.ToList<TestEntity>();
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
            Query<TestEntity> query = new Query<TestEntity>(connection);
            query.Where(v => v.Name.Contains("FD2"));
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
            Query<TestEntity> query = new Query<TestEntity>(connection);
            query.Where(v => v.Name.Contains("FD2"));
            IList<TestEntity> data = await query.ToListAsync<TestEntity>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 实例化数组Contains测试
        /// </summary>
        [TestMethod]
        public void QueryNewArrayContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            query.Where(v => new[] { "0C53AEDA46DC957B9EDD", "70F266168B87F90A972C" }.Contains(v.Name));
            IEnumerable<TestEntity> data = query.ToList<TestEntity>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 实例化集合Contains测试
        /// </summary>
        [TestMethod]
        public void QueryNewListContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            query.Where(v => new List<string> { "0C53AEDA46DC957B9EDD", "70F266168B87F90A972C" }.Contains(v.Name));
            IEnumerable<TestEntity> data = query.ToList<TestEntity>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void QueryListContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IList<string> values = new List<string> { "0C53AEDA46DC957B9EDD", "70F266168B87F90A972C" };
            query.Where(v => values.Contains(v.Name));
            IEnumerable<TestEntity> data = query.ToList<TestEntity>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void QueryArrayContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            string[] values = new string[] { "0C53AEDA46DC957B9EDD", "70F266168B87F90A972C" };
            query.Where(v => values.Contains(v.Name));
            IEnumerable<TestEntity> data = query.ToList<TestEntity>();
            Assert.IsTrue(data.Any());
        }


        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void QueryIEnumerableContainsTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IEnumerable<string> values = new[] { "0C53AEDA46DC957B9EDD", "70F266168B87F90A972C" };
            query.Where(v => values.Contains(v.Name));
            IEnumerable<TestEntity> data = query.ToList<TestEntity>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void QueryBoolNotTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            query.Where(v => !v.IsDelete);
            IEnumerable<TestEntity> data = query.ToList<TestEntity>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void QueryBoolAccessTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            query.Where(v => v.IsDelete);
            IEnumerable<TestEntity> data = query.ToList<TestEntity>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 查询测试
        /// </summary>
        [TestMethod]
        public void QueryParamQueryTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            QueryParam queryParam = new QueryParam
            {
                IsDelete = false,
                CreateTime = DateTime.Now.AddDays(-1)
            };
            query.Where(v => v.IsDelete == queryParam.IsDelete && v.Logged > queryParam.CreateTime);
            IEnumerable<TestEntity> data = query.ToList<TestEntity>();
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
                Query<TestEntity> query = new Query<TestEntity>(connection);
                QueryParam queryParam = new QueryParam
                {
                    IsDelete = false,
                    CreateTime = DateTime.Now.AddDays(-1),
                    Key = "DB"
                };
                query.Where(v => !v.IsDelete && (v.Name.Contains(queryParam.Key) || v.Logged > queryParam.CreateTime));
                IEnumerable<TestEntity> data = query.ToList<TestEntity>();
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
            Expression<Func<TestEntity, bool>> where = v => v.Type == TestType.Log && (v.IsDelete == false || v.Message.Contains("ab")) && (v.Code.Contains("abc") || v.Number == "ab") && (v.Logged > new DateTime(2021, 3, 12) || v.UpdateTime < DateTime.Now.Date);
            Query<TestEntity> query = new Query<TestEntity>(connection);
            query.Where(where);
            IEnumerable<TestEntity> data = query.ToList<TestEntity>();
            Assert.IsTrue(data.Any());
        }

        /// <summary>
        /// 生成Guid查询测试
        /// </summary>
        [TestMethod]
        public void QueryNewGuidTest()
        {
            using IDbConnection connection = CreateConnection();
            Expression<Func<TestEntity, bool>> where = v => v.Id == Guid.NewGuid();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            query.Where(where);
            IEnumerable<TestEntity> data = query.ToList<TestEntity>();
            Assert.IsFalse(data.Any());
        }

        /// <summary>
        /// 获取单个测试
        /// </summary>
        [TestMethod]
        public void GetTest()
        {
            using IDbConnection connection = CreateConnection();
            TestEntity testEntity = connection.Get<TestEntity>(Guid.Parse("0007dad3-eedd-44ee-b732-764ecdcaa432"));
            Assert.IsNotNull(testEntity);
        }

        /// <summary>
        /// 获取单个测试
        /// </summary>
        [TestMethod]
        public async Task GetAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            TestEntity testEntity = await connection.GetAsync<TestEntity>(Guid.Parse("0007dad3-eedd-44ee-b732-764ecdcaa432"));
            Assert.IsNotNull(testEntity);
        }

        /// <summary>
        /// 获取单个测试
        /// </summary>
        [TestMethod]
        public async Task GetNewGuidAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            TestEntity testEntity = await connection.GetAsync<TestEntity>(Guid.NewGuid());
            Assert.IsNull(testEntity);
        }

        /// <summary>
        /// 获取单个测试
        /// </summary>
        [TestMethod]
        public void FirstOrDefaultTest()
        {
            using IDbConnection connection = CreateConnection();
            TestEntity testEntity = connection.Query<TestEntity>().Where(f => f.Name.Contains("CDF")).FirstOrDefault<TestEntity>();
            Assert.IsNotNull(testEntity);
        }

        /// <summary>
        /// 获取单个测试
        /// </summary>
        [TestMethod]
        public void FirstOrDefaultToModelTest()
        {
            using IDbConnection connection = CreateConnection();
            TestEntityModel testEntity = connection.Query<TestEntity>().Where(f => f.Name.Contains("CDF")).FirstOrDefault<TestEntityModel>();
            Assert.IsNotNull(testEntity);
        }

        /// <summary>
        /// 获取单个测试
        /// </summary>
        [TestMethod]
        public async Task FirstOrDefaultAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            TestEntity testEntity = await connection.Query<TestEntity>().Where(f => f.Name.Contains("CDF")).FirstOrDefaultAsync<TestEntity>();
            Assert.IsNotNull(testEntity);
        }


        /// <summary>
        /// 获取单个测试
        /// </summary>
        [TestMethod]
        public void AnyTest()
        {
            using IDbConnection connection = CreateConnection();
            bool result = connection.Query<TestEntity>().Where(f => f.Name.Contains("CDF")).Any();
            Assert.IsTrue(result);
        }

        /// <summary>
        /// 获取单个测试
        /// </summary>
        [TestMethod]
        public async Task AnyAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            bool result = await connection.Query<TestEntity>().Where(f => f.Name.Contains("CDF")).AnyAsync();
            Assert.IsTrue(result);
        }

        /// <summary>
        /// 获取全部测试
        /// </summary>
        [TestMethod]
        public async Task GetAllAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            IEnumerable<TestEntity> testEntity = await connection.GetAllAsync<TestEntity>();
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
            IEnumerable<TestEntity> testEntity = connection.GetAll<TestEntity>();
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
            int total = connection.GetCount<TestEntityBak>();
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
            int total = connection.GetCount<TestEntity>();
            Assert.IsTrue(total > 0);
        }

        /// <summary>
        /// 获取数量测试
        /// </summary>
        [TestMethod]
        public async Task GetCountAsyncNoDataTest()
        {
            using IDbConnection connection = CreateConnection();
            int total = await connection.GetCountAsync<TestEntityBak>();
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
            int total = await connection.GetCountAsync<TestEntity>();
            Assert.IsTrue(total > 0);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        [TestMethod]
        public async Task QueryPageTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            query.TakePage(1, 10);
            IList<TestEntity> result = await query.ToListAsync<TestEntity>();
            Assert.AreEqual(10, result.Count);
        }

        /// <summary>
        /// 获取指定数量
        /// </summary>
        [TestMethod]
        public async Task QueryTakeTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IEnumerable<TestEntity> result = await query.Take(100).ToListAsync<TestEntity>();
            Assert.AreEqual(100, result.Count());
        }

        /// <summary>
        /// 排序
        /// </summary>
        [TestMethod]
        public async Task QueryOrderTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            query.TakePage(1, 10).OrderBy("Logged DESC");
            IList<TestEntity> result = await query.ToListAsync<TestEntity>();
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// 排序分组
        /// </summary>
        [TestMethod]
        public async Task QueryGroupTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            query.TakePage(1, 10).GroupBy("Logged");
            IList<TestEntity> result = await query.ToListAsync<TestEntity>();
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// 排序分组
        /// </summary>
        [TestMethod]
        public async Task QueryGroupHavingTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IEnumerable<TestEntity> result = await query.TakePage(1, 10).GroupBy("Logged").Having(f => f.Logged > DateTime.Now).ToListAsync<TestEntity>();
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// 排序分组
        /// </summary>
        [TestMethod]
        public async Task QueryDateTimeDateTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IEnumerable<TestEntity> result = await query.Where(f => f.Logged.Date > DateTime.Now.Date).ToListAsync<TestEntity>();
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// 多个排序
        /// </summary>
        [TestMethod]
        public async Task QueryMultiOrderTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            query.TakePage(1, 10).OrderBy("Logged desc").OrderBy("Name asc");
            IEnumerable<TestEntity> result = await query.ToListAsync<TestEntity>();
            Assert.IsTrue(result.Any());
        }

        /// <summary>
        /// 多个排序
        /// </summary>
        [TestMethod]
        public async Task QueryMultiOrderAndGroupTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            query.TakePage(1, 10).OrderBy("Logged desc").OrderBy("Name asc").GroupBy("Logged");
            IEnumerable<TestEntity> result = await query.ToListAsync<TestEntity>();
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public void SelectObjectTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IList<TestEntityAliasModel> models = query.Where(f => f.Name.Contains("ABCD")).Select(f => new
            {
                f.Id,
                B = f.Name,
                C = f.Logged,
                D = f.IsDelete,
                E = f.UpdateTime,
                F = f.Code,
                G = f.Number,
                H = f.Message,
                I = f.Type,
                J = f.Ignore
            }).ToList<TestEntityAliasModel>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any(f => f.B.Contains("ABCD")));
        }


        [TestMethod]
        public void SelectCacheTest()
        {
            for (int i = 0; i < 100; i++)
            {
                using IDbConnection connection = CreateConnection();
                Query<TestEntity> query = new Query<TestEntity>(connection);
                query.Select(f => new
                {
                    f.Id,
                    B = f.Name,
                    C = f.Logged,
                    D = f.IsDelete,
                    E = f.UpdateTime,
                    F = f.Code,
                    G = f.Number,
                    H = f.Message,
                    I = f.Type,
                    J = f.Ignore
                });
                query.Select(f => new
                {
                    f.Id,
                    B = f.Name,
                    C = f.Logged,
                    D = f.IsDelete,
                    F = f.Code,
                    G = f.Number,
                    H = f.Message,
                    I = f.Type,
                    J = f.Ignore
                });

                query.Select(f => new
                {
                    f.Id,
                    B = f.Name,
                    C = f.Logged,
                    D = f.IsDelete,
                    E = f.UpdateTime,
                    F = f.Code,
                    H = f.Message,
                    I = f.Type,
                    J = f.Ignore
                });
            }
        }

        [TestMethod]
        public void SelectEntityTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IList<TestEntityModel> models = query.Where(f => f.Name.Contains("ACD")).Select(f => new TestEntity { Id = f.Id, Ignore = f.Ignore, Name = f.Name, Code = f.Code, IsDelete = f.IsDelete }).ToList<TestEntityModel>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any(f => f.Name.Contains("ACD")));
        }


        [TestMethod]
        public void SelectNullableGuidTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IList<Guid?> models = query.Where(f => f.Name.Contains("ACD")).Select(f => f.RequestId).ToList<Guid?>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any(f => !f.HasValue));
        }

        [TestMethod]
        public void SelectDistinctNameTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IList<string> models = query.Where(f => f.Name == "QueryExtensionsTests").Select(f => f.Name).Distinct().ToList<string>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Count == 1);
        }

        [TestMethod]
        public void EqualsTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IList<string> models = query.Where(f => f.Name.Equals("QueryExtensionsTests")).Select(f => f.Name).Distinct().ToList<string>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Count == 1);
        }

        [TestMethod]
        public void GreaterThanTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IList<TestEntity> models = query.Where(f => f.Version > 10).ToList<TestEntity>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }


        [TestMethod]
        public void ExpressionOrderByTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IList<TestEntity> models = query.Where(f => f.Version > 10).OrderBy(f => new { f.Name, f.Code }).OrderBy(f => f.Logged).OrderBy(f => new TestEntity { IsDelete = f.IsDelete, Type = f.Type, Ignore = f.Ignore }).ToList<TestEntity>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void ExpressionOrderByDescendingTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IList<TestEntity> models = query.Where(f => f.Version > 10).OrderBy(f => new { f.Name, f.Code }).OrderByDescending(f => f.Logged).OrderByDescending(f => new TestEntity { IsDelete = f.IsDelete, Type = f.Type, Ignore = f.Ignore }).ToList<TestEntity>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void GroupByTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IList<TestEntity> models = query.Where(f => f.Version > 10).OrderBy(f => new { f.Name, f.Code }).GroupBy(f => f.Name).GroupBy(f => new { f.Code, f.IsDelete }).GroupBy(f => new TestEntity { Logged = f.Logged }).ToList<TestEntity>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void HavingTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IList<TestEntity> models = query.Where(f => f.Version > 10).OrderBy(f => new { f.Name, f.Code }).GroupBy(f => f.Name).GroupBy(f => new { f.Code, f.IsDelete }).Having(f => f.Code.Contains("A")).ToList<TestEntity>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void WhereIsNullTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IList<TestEntity> models = query.Where(f => f.RequestId == null).ToList<TestEntity>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void WhereIsNotNullTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IList<TestEntity> models = query.Where(f => f.RequestId != null).ToList<TestEntity>();
            Assert.IsNotNull(models);
            Assert.IsTrue(models.Any());
        }

        [TestMethod]
        public void MaxStringTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            string max = query.Where(f => f.RequestId != null).Max(f => f.Code);
            Assert.IsNotNull(max);
            Console.WriteLine(max);
        }

        [TestMethod]
        public void MaxIntTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            int max = query.Where(f => f.RequestId != null).Max(f => f.Version);
            Assert.IsTrue(max > 0);
            Console.WriteLine(max);
        }


        [TestMethod]
        public void MaxNullableTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntityBak> query = new Query<TestEntityBak>(connection);
            int? max = query.Where(f => f.RequestId != null).Max(f => (int?)f.Version);
            Assert.IsFalse(max.HasValue);
        }

        [TestMethod]
        public void MaxStringNullableTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntityBak> query = new Query<TestEntityBak>(connection);
            string max = query.Where(f => f.RequestId != null).Max(f => f.Name);
            Assert.IsNull(max);
        }

        [TestMethod]
        public async Task MaxNullableAsyncTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntityBak> query = new Query<TestEntityBak>(connection);
            DateTime? max = await query.Where(f => f.RequestId != null).MaxAsync(f => f.UpdateTime);
            Assert.IsFalse(max.HasValue);
        }

        [TestMethod]
        public async Task MaxNullableNoDataAsyncTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntityBak> query = new Query<TestEntityBak>(connection);
            int? max = await query.Where(f => f.RequestId != null).MaxAsync(f => (int?)f.Version);
            Assert.IsFalse(max > 0);
        }


        [TestMethod]
        public void MinIntTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            int max = query.Where(f => f.RequestId != null).Min(f => f.Version);
            Assert.IsTrue(max == 0);
            Console.WriteLine(max);
        }


        [TestMethod]
        public void MinNullableTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntityBak> query = new Query<TestEntityBak>(connection);
            DateTime? max = query.Where(f => f.RequestId != null).Min(f => f.UpdateTime);
            Assert.IsFalse(max.HasValue);
        }

        [TestMethod]
        public async Task MinNullableAsyncTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntityBak> query = new Query<TestEntityBak>(connection);
            DateTime? max = await query.Where(f => f.RequestId != null).MinAsync(f => f.UpdateTime);
            Assert.IsFalse(max.HasValue);
        }


        [TestMethod]
        public void SumIntTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            int max = query.Where(f => f.RequestId != null).Sum(f => f.Version);
            Assert.IsTrue(max == 0);
            Console.WriteLine(max);
        }


        [TestMethod]
        public void SumNullableTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntityBak> query = new Query<TestEntityBak>(connection);
            int? max = query.Where(f => f.RequestId != null).Sum(f => (int?)f.Version);
            Assert.IsFalse(max > 0);
        }

        [TestMethod]
        public async Task SumAsyncTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            int max = await query.Where(f => f.RequestId != null).SumAsync(f => f.Version);
            Assert.IsTrue(max > 0);
        }

        [TestMethod]
        public void NotSupportTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntityBak> query = new Query<TestEntityBak>(connection);
            Assert.ThrowsException<NotSupportedException>(() => query.Where(f => f.RequestId != null).Sum(f => f.UpdateTime));
        }

        [TestMethod]
        public void DateAddYearsTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IList<TestEntity> entities = query.Where(f => f.Logged > DateTime.Now.AddYears(-2).AddYears(1)).ToList<TestEntity>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void DateAddMonthsTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IList<TestEntity> entities = query.Where(f => f.Logged > DateTime.Now.AddMonths(-2)).ToList<TestEntity>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void DateAddDaysTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IList<TestEntity> entities = query.Where(f => f.Logged > DateTime.Now.AddDays(-20)).ToList<TestEntity>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void DateAddHoursTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            IList<TestEntity> entities = query.Where(f => f.Logged > DateTime.Now.Date.AddHours(-500)).ToList<TestEntity>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void DateAddMinutesTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            DateTime date = DateTime.Now;
            IList<TestEntity> entities = query.Where(f => f.Logged > date.Date.AddMinutes(-500 * 60)).ToList<TestEntity>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void DateAddSecondsTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            DateTime date = DateTime.Now;
            IList<TestEntity> entities = query.Where(f => f.Logged > date.Date.AddSeconds(-500 * 60 * 60)).ToList<TestEntity>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void DateAddMillisecondsTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            DateTime date = DateTime.Now;
            IList<TestEntity> entities = query.Where(f => f.Logged > date.Date.AddMilliseconds(-500 * 60 * 60 * 1000)).ToList<TestEntity>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void NullableParamTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            DateTime? date = DateTime.Now;
            IList<TestEntity> entities = query.Where(f => f.Logged > date.Value).ToList<TestEntity>();
            Assert.IsTrue(entities.Any());
        }

        [TestMethod]
        public void NullableDateTimeValueDateTest()
        {
            Console.WriteLine(typeof(TestEntity).TypeHandle.Value);
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = new Query<TestEntity>(connection);
            DateTime? date = DateTime.Now.AddDays(-20);
            IList<TestEntity> entities = query.Where(f => f.Logged > date.Value.Date).ToList<TestEntity>();
            Assert.IsTrue(entities.Any());
        }

        #endregion
    }


    public class QueryParam
    {
        public DateTime? CreateTime { get; set; }

        public bool? IsDelete { get; set; }

        public string Key { get; set; }
    }

    [Table("testentity_1")]
    public class TestEntityBak : IEntity
    {
        [ExplicitKey]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid? RequestId { get; set; }

        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }
    }

    [Table("testentity")]
    public class TestEntity : IEntity
    {
        [ExplicitKey]
        public Guid Id { get; set; }

        public Guid? RequestId { get; set; }

        public string Name { get; set; }

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

        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }
    }

    public class TestEntityModel : IEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime Logged { get; set; }

        public bool IsDelete { get; set; }

        public DateTime? UpdateTime { get; set; }

        public string Message { get; set; }

        public string Code { get; set; }

        public string Number { get; set; }

        public TestType Type { get; set; }

        public string Ignore { get; set; }

        public string TypeDesc => Type.ToString();
    }

    public class TestEntityAliasModel : IEntity
    {
        public Guid Id { get; set; }

        public string B { get; set; }

        public DateTime C { get; set; }

        public bool D { get; set; }

        public DateTime? E { get; set; }

        public string F { get; set; }

        public string G { get; set; }

        public string H { get; set; }

        public TestType I { get; set; }

        public string J { get; set; }

        public string K => I.ToString();
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

    public static class TestTool
    {
        public static bool IsDelete(TestEntity testEntity)
        {
            return testEntity.Name.Contains("a");
        }
    }
}