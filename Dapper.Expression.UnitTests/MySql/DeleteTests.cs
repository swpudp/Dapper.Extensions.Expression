using Dapper.Extensions.Expression;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Dapper.Extensions.Expression.UnitTests.MySql
{
    [TestClass]
    public class DeleteTests : MysqlBaseTest
    {
        /// <summary>
        /// 删除测试
        /// </summary>
        [TestMethod]
        public void DeleteTest()
        {
            Log log = ObjectUtils.CreateLogs(1).First();
            int value = Execute(connection => connection.Insert(log));
            Assert.AreEqual(1, value);
            int deleted = Execute(connection => connection.Delete(log));
            Assert.IsTrue(deleted > 0);
        }

        /// <summary>
        /// 删除测试
        /// </summary>
        [TestMethod]
        public async Task DeleteAsyncTest()
        {
            Log log = ObjectUtils.CreateLogs(1).First();
            int deleted = await Execute(connection => connection.DeleteAsync(log));
            Assert.IsFalse(deleted > 0);
            int result = await Execute(connection => connection.InsertAsync(log));
            Assert.IsTrue(result > 0);
            deleted = await Execute(connection => connection.DeleteAsync(log));
            Assert.IsTrue(deleted > 0);
        }

        /// <summary>
        /// 删除测试
        /// </summary>
        [TestMethod]
        public void DeleteAllTest()
        {
            IList<Log> data = ObjectUtils.CreateLogs(100).ToList();
            Execute(connection => connection.InsertBulk(data));
            int deleted = Execute(connection => connection.DeleteAll<Log>());
            Assert.IsTrue(deleted > 0);
            int total = Execute(connection => connection.GetCount<Log>());
            Assert.AreEqual(0, total);
        }

        /// <summary>
        /// 删除测试
        /// </summary>
        [TestMethod]
        public async Task DeleteAllAsyncTest()
        {
            IList<Log> data = ObjectUtils.CreateLogs(10).ToList();
            await Execute(connection => connection.InsertBulkAsync(data));
            int deleted = await Execute(connection => connection.DeleteAllAsync<Log>());
            Assert.IsTrue(deleted > 0);
            int total = await Execute(connection => connection.GetCountAsync<Log>());
            Assert.AreEqual(0, total);
        }

        /// <summary>
        /// 删除测试
        /// </summary>
        [TestMethod]
        public void DeleteByExpressionTest()
        {
            IList<Log> data = ObjectUtils.CreateLogs(100).ToList();
            Execute(connection => connection.InsertBulk(data));
            int deleted = Execute(connection => connection.Delete<Log>(f => f.LogType == LogType.Trace));
            Assert.IsTrue(deleted > 0);
        }

        /// <summary>
        /// 删除测试
        /// </summary>
        [TestMethod]
        public async Task DeleteByExpressionAsyncTest()
        {
            IList<Log> data = ObjectUtils.CreateLogs(100).ToList();
            await Execute(connection => connection.InsertBulkAsync(data));
            int deleted = await Execute(connection => connection.DeleteAsync<Log>(f => f.LogType == LogType.Trace));
            Assert.IsTrue(deleted > 0);
        }
    }
}