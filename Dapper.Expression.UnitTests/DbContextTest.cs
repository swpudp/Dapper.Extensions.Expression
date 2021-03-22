using System;
using System.Collections.Generic;
using System.Data;
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

            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("")));


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
        public void DateTimeDiffTest()
        {
            DbContext ctx = new MySqlContext(new DbConnectionFactory(() => new MySqlConnection("server=127.0.0.1;port=3306;database=invoicecloud;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8")));
            var list = ctx.Query<Log>().Where(f => (DateTime.Now - f.Logged).TotalDays > 1000).ToList();
            Assert.IsFalse(list.Any());
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

        public int ThreadId { get; set; }

        public bool IsDelete { get; set; }

        public DateTime? UpdateTime { get; set; }
    }
}
