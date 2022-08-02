using Dapper.Extensions.Expression;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using IsolationLevel = System.Data.IsolationLevel;

namespace Dapper.Extensions.Expression.UnitTests.MySql
{
    [TestClass]
    public class InsertTests : MysqlBaseTest
    {
        [TestMethod]
        public void InsertBulkTest()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Buyer buyer = CreateBuyer();
            IList<Order> orders = CreateOrders(100, 10, buyer).ToList();
            IList<Item> items = CreateItems(orders).ToList();
            IList<Attachment> attachments = CreateAttachments(orders).ToList();
            Execute(connection => connection.Insert(buyer));
            int orderCount = Execute(connection => connection.InsertBulk(orders));
            int itemCount = Execute(connection => connection.InsertBulk(items));
            int attachmentCount = Execute(connection => connection.InsertBulk(attachments));
            stopwatch.Stop();
            Console.WriteLine("InsertBulk写入value1={0}条,耗时{1}", orderCount + itemCount + attachmentCount, stopwatch.Elapsed);
            Assert.AreEqual(orderCount, orders.Count);
            Assert.AreEqual(itemCount, items.Count);
            Assert.AreEqual(attachmentCount, attachments.Count);
        }

        [TestMethod]
        public void InsertBulkPartFailTest()
        {
            Assert.ThrowsException<MySqlException>(() =>
            {
                IList<Order> orders = Enumerable.Range(0, 50).Select((f, index) => new Order
                {
                    Id = Guid.NewGuid(),
                    IsDelete = false
                }).ToList();
                return Execute(connection => connection.InsertBulk(orders));
            });
        }

        [TestMethod]
        public void InsertBulkPartFailWithDbTransactionTest()
        {
            IDbConnection connection = CreateConnection();
            connection.Open();
            IDbTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
            Buyer buyer = CreateBuyer();
            IList<Order> orders = CreateOrders(500, 100, buyer).ToList();
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                int v = connection.Insert(buyer);
                Assert.IsTrue(v > 0);
                int value1 = connection.InsertBulk(orders);
                transaction.Commit();
                stopwatch.Stop();
                Assert.IsTrue(value1 > 0);
                Assert.AreNotEqual(value1, orders.Count);
            }
            catch
            {
                transaction.Rollback();
            }
            bool exist = Execute(c => c.Query<Buyer>().Where(f => f.Id == buyer.Id).Any());
            Assert.IsFalse(exist);
            IEnumerable<Guid> ids = orders.Select(f => f.Id);
            bool existOrder = Execute(c => c.Query<Order>().Where(f => ids.Contains(f.Id)).Any());
            Assert.IsFalse(existOrder);
        }

        [TestMethod]
        public void InsertBulkPartFailWithTransactionScopeTest()
        {
            Buyer buyer = CreateBuyer();
            IList<Order> orders = CreateOrders(50, 50, buyer).ToList();
            Assert.ThrowsException<MySqlException>(() =>
            {
                using var trans = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted });
                Execute(connection => connection.Insert(buyer));
                Execute(connection => connection.InsertBulk(orders));
                trans.Complete();
            });
            bool exist = Execute(connection => connection.Query<Buyer>().Where(f => f.Id == buyer.Id).Any());
            Assert.IsFalse(exist);
            IEnumerable<Guid> ids = orders.Select(f => f.Id);
            bool existOrder = Execute(connection => connection.Query<Order>().Where(f => ids.Contains(f.Id)).Any());
            Assert.IsFalse(existOrder);
        }

        /// <summary>
        /// 写入单条测试
        /// </summary>

        [TestMethod]
        public void InsertOneTest()
        {
            Buyer buyer = CreateBuyer();
            Execute(connection => connection.Insert(buyer));
            Order insertEntity = CreateOrders(1, 1, buyer).First();
            int value = Execute(connection => connection.Insert(insertEntity));
            Assert.AreEqual(1, value);
        }

        /// <summary>
        /// 写入多条测试
        /// </summary>
        [TestMethod]
        public void InsertManyTest()
        {
            Buyer buyer = CreateBuyer();
            Execute(connection => connection.Insert(buyer));
            IList<Order> orders = CreateOrders(100, 10, buyer).ToList();
            int values = Execute(connection => connection.Insert(orders));
            Assert.AreEqual(values, orders.Count);
        }

        /// <summary>
        /// 写入多条部分失败测试
        /// </summary>
        [TestMethod]
        public void InsertManyPartFailTest()
        {
            Buyer buyer = CreateBuyer();
            IList<Order> orders = CreateOrders(100, 100, buyer).ToList();
            Assert.ThrowsException<MySqlException>(() =>
            {
                Execute(connection => connection.Insert(buyer));
                return Execute(connection => connection.Insert(orders));
            });
            bool exist = Execute(connection => connection.Query<Buyer>().Where(f => f.Id == buyer.Id).Any());
            Assert.IsTrue(exist);
            IEnumerable<Guid> ids = orders.Select(f => f.Id);
            bool existOrder = Execute(connection => connection.Query<Order>().Where(f => ids.Contains(f.Id)).Any());
            Assert.IsTrue(existOrder);
        }

        /// <summary>
        /// 事务写入多条部分失败测试
        /// </summary>
        [TestMethod]
        public void InsertManyPartFailUseDbTransactionTest()
        {
            Buyer buyer = CreateBuyer();
            IList<Order> orders = CreateOrders(100, 100, buyer).ToList();
            Assert.ThrowsException<MySqlException>(() => ExecuteTransaction((connection, transaction) => new[] { connection.Insert(buyer, transaction), connection.InsertBulk(orders, transaction) }));
            bool exist = Execute(connection => connection.Query<Buyer>().Where(f => f.Id == buyer.Id).Any());
            Assert.IsFalse(exist);
            IEnumerable<Guid> ids = orders.Select(f => f.Id);
            bool existOrder = Execute(connection => connection.Query<Order>().Where(f => ids.Contains(f.Id)).Any());
            Assert.IsFalse(existOrder);
        }

        /// <summary>
        /// 事务写入多条部分失败测试
        /// </summary>
        [TestMethod]
        public void InsertManyPartFailUseTransactionScopeTest()
        {
            Buyer buyer = CreateBuyer();
            IList<Order> orders = CreateOrders(100, 100, buyer).ToList();
            Assert.ThrowsException<MySqlException>(() =>
            {
                using TransactionScope trans = new TransactionScope();
                Execute(connection => connection.Insert(buyer));
                Execute(connection => connection.Insert(orders));
                trans.Complete();
            });
            bool exist = Execute(connection1 => connection1.Query<Buyer>().Where(f => f.Id == buyer.Id).Any());
            Assert.IsFalse(exist);
            IEnumerable<Guid> ids = orders.Select(f => f.Id);
            bool existOrder = Execute(connection1 => connection1.Query<Order>().Where(f => ids.Contains(f.Id)).Any());
            Assert.IsFalse(existOrder);
        }

        /// <summary>
        /// 事务写入多条成功
        /// </summary>
        [TestMethod]
        public void InsertManySuccessUseTransactionTest()
        {
            Buyer buyer = CreateBuyer();
            IList<Order> orders = CreateOrders(100, 10, buyer).ToList();
            IList<Item> items = CreateItems(orders).ToList();
            int values = ExecuteTransaction((connection, transaction) => new[] { connection.Insert(buyer, transaction), connection.Insert(orders, transaction), connection.Insert(items, transaction) });
            Assert.IsTrue(values > 0);
            bool exist = Execute(connection => connection.Query<Buyer>().Where(f => f.Id == buyer.Id).Any());
            Assert.IsTrue(exist);
            IEnumerable<Guid> orderIds = orders.Select(f => f.Id);
            int count = Execute(connection => connection.Query<Order>().Where(f => orderIds.Contains(f.Id)).Count());
            Assert.AreEqual(orders.Count, count);
            IEnumerable<Guid> itemIds = items.Select(f => f.Id);
            count = Execute(connection => connection.Query<Item>().Where(f => itemIds.Contains(f.Id)).Count());
            Assert.AreEqual(items.Count, count);
        }

        /// <summary>
        /// 事务写入多条成功测试
        /// </summary>
        [TestMethod]
        public void InsertManySuccessUseTransactionScopeTest()
        {
            Buyer buyer = CreateBuyer();
            IList<Order> orders = CreateOrders(100, 10, buyer).ToList();
            IList<Item> items = CreateItems(orders).ToList();
            using (TransactionScope trans = new TransactionScope())
            {
                Execute(connection => connection.Insert(buyer));
                Execute(connection => connection.Insert(orders));
                Execute(connection => connection.Insert(items));
                trans.Complete();
            }
            bool exist = Execute(connection1 => connection1.Query<Buyer>().Where(f => f.Id == buyer.Id).Any());
            Assert.IsTrue(exist);
            IEnumerable<Guid> orderIds = orders.Select(f => f.Id);
            int count = Execute(connection1 => connection1.Query<Order>().Where(f => orderIds.Contains(f.Id)).Count());
            Assert.AreEqual(orders.Count, count);
            IEnumerable<Guid> itemIds = items.Select(f => f.Id);
            count = Execute(connection1 => connection1.Query<Item>().Where(f => itemIds.Contains(f.Id)).Count());
            Assert.AreEqual(items.Count, count);
        }

        /// <summary>
        /// 事务批量写入成功
        /// </summary>
        [TestMethod]
        public void InsertBulkSuccessUseTransactionTest()
        {
            Buyer buyer = CreateBuyer();
            IList<Order> orders = CreateOrders(100, 10, buyer).ToList();
            IList<Item> items = CreateItems(orders).ToList();
            ExecuteTransaction((connection, transaction) => new[] { connection.Insert(buyer, transaction), connection.InsertBulk(orders, transaction), connection.InsertBulk(items, transaction) });
            bool exist = Execute(connection => connection.Query<Buyer>().Where(f => f.Id == buyer.Id).Any());
            Assert.IsTrue(exist);
            IEnumerable<Guid> orderIds = orders.Select(f => f.Id);
            int count = Execute(connection => connection.Query<Order>().Where(f => orderIds.Contains(f.Id)).Count());
            Assert.AreEqual(orders.Count, count);
            IEnumerable<Guid> itemIds = items.Select(f => f.Id);
            count = Execute(connection => connection.Query<Item>().Where(f => itemIds.Contains(f.Id)).Count());
            Assert.AreEqual(items.Count, count);
        }

        /// <summary>
        /// 事务批量写入成功测试
        /// </summary>
        [TestMethod]
        public void InsertBulkSuccessUseTransactionScopeTest()
        {
            Buyer buyer = CreateBuyer();
            IList<Order> orders = CreateOrders(100, 10, buyer).ToList();
            IList<Item> items = CreateItems(orders).ToList();
            using (TransactionScope trans = new TransactionScope())
            {
                Execute(connection => connection.Insert(buyer));
                Execute(connection => connection.InsertBulk(orders));
                Execute(connection => connection.InsertBulk(items));
                trans.Complete();
            }
            bool exist = Execute(connection1 => connection1.Query<Buyer>().Where(f => f.Id == buyer.Id).Any());
            Assert.IsTrue(exist);
            IEnumerable<Guid> orderIds = orders.Select(f => f.Id);
            int count = Execute(connection1 => connection1.Query<Order>().Where(f => orderIds.Contains(f.Id)).Count());
            Assert.AreEqual(orders.Count, count);
            IEnumerable<Guid> itemIds = items.Select(f => f.Id);
            count = Execute(connection1 => connection1.Query<Item>().Where(f => itemIds.Contains(f.Id)).Count());
            Assert.AreEqual(items.Count, count);
        }

        /// <summary>
        /// 异步写入测试
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task InsertOneAsyncTest()
        {
            Buyer buyer = CreateBuyer();
            int result = await Execute(connection => connection.InsertAsync(buyer));
            Assert.IsTrue(result > 0);
        }

        /// <summary>
        /// 异步写入多条测试
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task InsertManyAsyncTest()
        {
            IEnumerable<Buyer> buyers = Enumerable.Range(0, 100).Select(f => CreateBuyer());
            int result = await Execute(connection => connection.InsertAsync(buyers));
            Assert.IsTrue(result > 0);
        }

        /// <summary>
        /// 异步批量写入测试
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task InsertBulkAsyncTest()
        {
            IList<Buyer> buyers = Enumerable.Range(0, 100).Select(f => CreateBuyer()).ToList();
            int result = await Execute(connection => connection.InsertBulkAsync(buyers));
            Assert.IsTrue(result > 0);
        }

        /// <summary>
        /// 异步批量写入测试
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task InsertBulkAsyncGivenTest()
        {
            Buyer buyer = CreateBuyer();
            IList<Order> orders = CreateOrders(10, 50, buyer).ToList();
            IList<string> serialNoList = new List<string> { "ABC", "BCD", "EFC", "C0A3", "A82639", "8064C0A3", "A8FD2639", "C0CDFA3" };
            foreach (var no in serialNoList)
            {
                int idx = serialNoList.IndexOf(no);
                orders[idx].SerialNo = no;
            }
            IList<Guid> idList = new List<Guid> { new Guid("001399e7-cacf-4323-8f18-75a9ef1480e0"), new Guid("009846a5-f96d-4583-bbac-d8c7056c1d2a") };
            foreach (var id in idList)
            {
                orders[idList.IndexOf(id)].Id = id;
            }
            await Execute(ctx => ctx.DeleteAsync<Order>(f => serialNoList.Contains(f.SerialNo)));
            await Execute(ctx => ctx.DeleteAsync<Order>(f => idList.Contains(f.Id)));
            await Execute(ctx => ctx.InsertAsync(buyer));
            int result = await Execute(connection => connection.InsertBulkAsync(orders));
            Assert.IsTrue(result > 0);
        }
    }
}