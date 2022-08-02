using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Dapper.Expressions.Mysql;
using Dapper.Extensions.Expression;
using Dapper.Extensions.Expression.Core;
using Dapper.Extensions.Expression.Infrastructure;
using Dapper.Extensions.Expression.Query;
using Dapper.Extensions.Expression.Query.Mapping;
using Dapper.Extensions.Expression.Query.QueryState;
using Dapper.Extensions.Expression.Query.Visitors;
using Dapper.Extensions.Expression.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;

namespace Dapper.Expression.UnitTests
{
    [TestClass]
    public class DbContextTest
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            Expression<Func<object>> test = () => new { a = 1, b = 2 };

            ParseExpression(test);


            Console.WriteLine(test.Body.NodeType);

            NewExpression body = test.Body as NewExpression;
            Console.WriteLine(body.NodeType);

            Console.WriteLine(test.NodeType);

            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySql.Data.MySqlClient.MySqlConnection("")));


            IQuery<Log> query = new Query<Log>(ctx, null, LockType.Unspecified);

            //query = query.Where(f => f.Id == new Guid("4a15a4a6-4045-11eb-b9b7-00ff736064f0"));

            //query = query.Where(f => f.Name.Contains("abc"));

            query = query.Where(f => f.ThreadId == 123);

            DateTime now = DateTime.Now;
            query = query.Where(f => f.Logged == now);

            Log paramLog = new Log { Name = "abc" };
            query = query.Where(f => f.IsDelete == TestTool.IsDelete(paramLog));

            IQueryState qs = QueryExpressionResolver.Resolve(query.QueryExpression, new ScopeParameterDictionary(), new StringSet());
            Console.WriteLine("Resolve");

            MappingData data = qs.GenerateMappingData();
            Console.WriteLine("GenerateMappingData");

            IDbExpressionTranslator translator = new DbExpressionTranslator();
            DbCommandInfo dbCommandInfo = translator.Translate(data.SqlQuery);
            Console.WriteLine("Translate");

            Assert.IsNotNull(dbCommandInfo);
            Console.WriteLine(dbCommandInfo.CommandText);
            Console.WriteLine(string.Join(",", dbCommandInfo.Parameters.Select(f => $"{f.Name}={f.Value}")));

            //IEnumerable<T> result = await DbContext.DatabaseProvider.CreateConnection().QueryAsync<T>(dbCommandInfo.CommandText, dbCommandInfo.GetParameters());

            //return result.ToList();

            new MySqlParameter("aaa", 123);


            Log log = await query.FirstOrDefaultAsync();

            Assert.IsNull(log);



        }

        private void ParseExpression(System.Linq.Expressions.Expression ex)
        {

        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public void InsertTest()
        {
            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8")));
            List<Log> logs = Enumerable.Range(0, 100).Select(f => new Log
            {
                Id = Guid.NewGuid(),
                IsDelete = false,
                Logged = DateTime.Now,
                Name = nameof(DbContextTest),
                TestType = TestType.Later,
                ThreadId = Thread.CurrentThread.ManagedThreadId
            }).ToList();
            ctx.InsertRange(logs);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public void UpdateTest()
        {
            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8")));

            ctx.Update<Order>(f => f.IsDelete && !f.IsDelete && !f.IsActive.Value, f => new Order
            {
                Version = f.Version - 1,
                UpdateTime = DateTime.Now
            });

            ctx.Update<Log>(f => f.Id == Guid.NewGuid(), f => new Log { Name = "123" });
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public void QueryTest()
        {
            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8")));
            //var list = ctx.Query<Log>().Where(f => f.IsDelete).ToList();
            IList<string> values = new List<string> { "0C53AEDA46DC957B9EDD", "70F266168B87F90A972C" };
            var list = ctx.Query<Log>().Where(f => values.Contains(f.Name)).OrderBy(f => f.Logged).ToList();
            Assert.IsFalse(list.Any());
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public void EqualsTest()
        {
            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8")));
            var list = ctx.Query<Log>().Where(f => f.Name.Equals("0C53AEDA46DC957B9EDD")).OrderBy(f => f.Logged).ToList();
            Assert.IsFalse(list.Any());
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public void DateTimeTest()
        {
            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8")));
            var list = ctx.Query<Log>().Where(f => f.Logged.Date > DateTime.Now.AddDays(-100).Date).ToList();
            Assert.IsFalse(list.Any());
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public void IsNullTest()
        {
            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8")));
            var list = ctx.Query<Log>().Where(f => f.UpdateTime == null).ToList();
            Assert.IsFalse(list.Any());
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public void DateAddDaysTest()
        {
            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8")));
            var list = ctx.Query<Log>().Where(f => f.UpdateTime == DateTime.Now.AddDays(-100).Date).ToList();
            Assert.IsFalse(list.Any());
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public void HasValueTest()
        {
            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8")));
            var list = ctx.Query<Log>().Where(f => f.UpdateTime.HasValue).ToList();
            Assert.IsFalse(list.Any());
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public void JoinTest()
        {
            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8")));
            var list = ctx.Query<Log>().LeftJoin<Log>((a, b) => a.Id == b.Id).Where((f, b) => f.UpdateTime.HasValue).Select((a, b) => a).ToList();
            Assert.IsFalse(list.Any());
        }

        [TestMethod]
        public void EnumListContainsTest()
        {
            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8")));
            IList<TestType> testTypes = new List<TestType> { TestType.Latest };
            var list = ctx.Query<Log>().Where(f => testTypes.Contains(f.TestType)).ToList();
            Assert.IsTrue(list.Any());
        }

        [TestMethod]
        public void SelectTest()
        {
            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8")));
            IList<TestType> testTypes = new List<TestType> { TestType.Latest };
            var list = ctx.Query<Log>().Where(f => testTypes.Contains(f.TestType)).Select(f => new { M = TestType.Later, Name = (f.ThreadId ?? 0).ToString() }).ToList();
            Assert.IsTrue(list.Any());
        }

        [TestMethod]
        public void NullableEnumEqualTest()
        {
            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8")));
            var list = ctx.Query<Log>().Where(f => f.ThreadId == 10).ToList();
            Assert.IsTrue(list.Any());
        }

        [TestMethod]
        public void ExistTest()
        {
            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8")));
            var list = ctx.Query<Log>().Where(f => ctx.Query<Log>().Where(x => x.Id == f.Id).Any()).ToList();
            Assert.IsTrue(list.Any());
        }

        [TestMethod]
        public void ConvertTest()
        {
            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8")));
            var list = ctx.Query<Log>().Where(f => ctx.Query<Log>().Where(x => x.ThreadId == new Random().Next() * 100).Any()).ToList();
            Assert.IsTrue(list.Any());
        }

        [TestMethod]
        public void BinaryTest()
        {
            Log log = new Log
            {
                Version = 123,
                Id = Guid.NewGuid()
            };
            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8")));
            var list = ctx.Update<Log>(f => f.Id == log.Id && f.Version == log.Version - 1, f => new Log
            {
                Version = f.Version + 1,
                Logged = DateTime.Now
            });
            Assert.IsTrue(list > 0);
        }


        [TestMethod]
        public void Segregate4Test()
        {
            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8")));
            ctx.Query<Order>().Where(f => (f.Status == Status.Running || f.IsDelete) && (f.CreateTime > DateTime.Now && f.SerialNo.Contains("abc") || f.Remark.Contains("abc")))
                .Select(f => f.Id)
                .ToList();
            //string expectSql =
            //    "SELECT `t1`.`Id` from `order` AS `t1` WHERE (`t1`.`Status`=@w_p_1 OR `t1`.`IsDelete`=@w_p_2) AND `t1`.`CreateTime`=NOW() OR (`t1`.`Number` LIKE @w_p_3 OR `t1`.`Remark` LIKE @w_p_4)";
            //Debug.WriteLine(sql);
            //Debug.WriteLine(expectSql);
            //Assert.IsTrue(string.Compare(TrimAllEmpty(sql), TrimAllEmpty(expectSql), StringComparison.OrdinalIgnoreCase) == 0);
        }


        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public async Task UpdateNullablePropertyAsyncTest()
        {
            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8")));
            Order order = new Order { DocId = null };
            Assert.IsNotNull(order);
            Guid docId = Guid.NewGuid();
            int updated = await ctx.UpdateAsync<Order>(f => f.DocId == order.DocId, f => new Order
            {
                Version = f.Version + 1,
                DocId = docId
            });
            Assert.IsTrue(updated > 0);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public async Task UpdateEmptyListTest()
        {
            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8")));
            Order order = new Order { DocId = null };
            IEnumerable<Guid> ids = Enumerable.Empty<Guid>();
            int updated = await ctx.UpdateAsync<Order>(f => ids.Contains(f.Id), f => new Order
            {
                Version = f.Version + 1
            });
            Assert.IsTrue(updated > 0);
        }

        /// <summary>
        /// SqlFunction测试
        /// </summary>
        [TestMethod]
        public async Task SqlFunctionTest()
        {
            await Task.CompletedTask;
            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8")));
            ctx.Query<Order>().Where(f => f.Status == Status.Running)
                .Select(f => new { f.Id, Max = Sql.Max(f.Amount), Count = Sql.Count(), X = Sql.Average(f.Version) })
                .ToList();
        }
    }

    public enum TestType
    {
        Latest,

        Later
    }

    public static class TestTool
    {
        public static bool IsDelete(Log log)
        {
            return log.Name.Contains("a");
        }
    }

    public class Log
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime Logged { get; set; }

        public TestType TestType { get; set; }

        public int? ThreadId { get; set; }

        public bool IsDelete { get; set; }

        public DateTime? UpdateTime { get; set; }

        public int Version { get; set; }
    }

    [Table("order")]
    public class Order
    {
        public Guid Id { get; set; }

        public Guid BuyerId { get; set; }

        [Column("Number")]
        public string SerialNo { get; set; }

        public string Remark { get; set; }

        public Status Status { get; set; }

        public SignState? SignState { get; set; }

        public decimal Amount { get; set; }
        public decimal? Freight { get; set; }

        public Guid? DocId { get; set; }

        public bool IsDelete { get; set; }

        public bool? IsActive { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }
    }

    public enum Status
    {
        Draft,
        Running,
        Stop
    }

    public enum SignState
    {
        UnSign,
        Signed
    }
}
