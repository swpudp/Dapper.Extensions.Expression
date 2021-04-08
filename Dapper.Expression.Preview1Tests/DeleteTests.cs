using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;


namespace Dapper.Extensions.Expression.UnitTests
{
    [TestClass]
    public class DeleteTests : BaseTest
    {
        /// <summary>
        /// 删除测试
        /// </summary>
        [TestMethod]
        public void DeleteTest()
        {
            using IDbConnection connection = CreateConnection();
            Log log = CreateLogs(1).First();
            int value = connection.Insert(log);
            Assert.AreEqual(1, value);
            int deleted = connection.Delete(log);
            Assert.IsTrue(deleted > 0);
        }

        /// <summary>
        /// 删除测试
        /// </summary>
        [TestMethod]
        public async Task DeleteAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            Log log = CreateLogs(1).First();
            int deleted = await connection.DeleteAsync(log);
            Assert.IsFalse(deleted > 0);
            int result = await connection.InsertAsync(log);
            Assert.IsTrue(result > 0);
            deleted = await connection.DeleteAsync(log);
            Assert.IsTrue(deleted > 0);
        }

        /// <summary>
        /// 删除测试
        /// </summary>
        [TestMethod]
        public void DeleteAllTest()
        {
            using IDbConnection connection = CreateConnection();
            IList<Log> data = CreateLogs(100).ToList();
            connection.InsertBulk(data);
            int deleted = connection.DeleteAll<Log>();
            Assert.IsTrue(deleted > 0);
            int total = connection.GetCount<Log>();
            Assert.AreEqual(0, total);
        }

        /// <summary>
        /// 删除测试
        /// </summary>
        [TestMethod]
        public async Task DeleteAllAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            IList<Log> data = CreateLogs(10).ToList();
            await connection.InsertBulkAsync(data);
            int deleted = await connection.DeleteAllAsync<Log>();
            Assert.IsTrue(deleted > 0);
            int total = await connection.GetCountAsync<Log>();
            Assert.AreEqual(0, total);
        }

        /// <summary>
        /// 删除测试
        /// </summary>
        [TestMethod]
        public void DeleteByExpressionTest()
        {
            using IDbConnection connection = CreateConnection();
            IList<Log> data = CreateLogs(100).ToList();
            connection.InsertBulk(data);
            int deleted = connection.Delete<Log>(f => f.LogType == LogType.Trace);
            Assert.IsTrue(deleted > 0);
        }

        /// <summary>
        /// 删除测试
        /// </summary>
        [TestMethod]
        public async Task DeleteByExpressionAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            IList<Log> data = CreateLogs(100).ToList();
            await connection.InsertBulkAsync(data);
            int deleted = await connection.DeleteAsync<Log>(f => f.LogType == LogType.Trace);
            Assert.IsTrue(deleted > 0);
        }
    }
}