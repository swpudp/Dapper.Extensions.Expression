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

namespace Dapper.Extensions.Expression.UnitTests
{
    [TestClass]
    public class InsertTests : BaseTest
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
            using IDbConnection connection = CreateConnection();
            connection.Insert(buyer);
            int orderCount = connection.InsertBulk(orders);
            int itemCount = connection.InsertBulk(items);
            int attachmentCount = connection.InsertBulk(attachments);
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
                using IDbConnection connection = CreateConnection();
                return connection.InsertBulk(orders);
            });
        }

        [TestMethod]
        public void InsertBulkPartFailWithDbTransactionTest()
        {
            using IDbConnection connection = CreateConnection();
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
            bool exist = connection.Query<Buyer>().Where(f => f.Id == buyer.Id).Any();
            Assert.IsFalse(exist);
            IEnumerable<Guid> ids = orders.Select(f => f.Id);
            bool existOrder = connection.Query<Order>().Where(f => ids.Contains(f.Id)).Any();
            Assert.IsFalse(existOrder);
        }

        [TestMethod]
        public void InsertBulkPartFailWithTransactionScopeTest()
        {
            Buyer buyer = CreateBuyer();
            IList<Order> orders = CreateOrders(50, 50, buyer).ToList();
            Assert.ThrowsException<MySqlException>(() =>
            {
                using IDbConnection connection = CreateConnection();
                using var trans = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted });
                connection.Insert(buyer);
                connection.InsertBulk(orders);
                trans.Complete();
            });
            using IDbConnection connection1 = CreateConnection();
            bool exist = connection1.Query<Buyer>().Where(f => f.Id == buyer.Id).Any();
            Assert.IsFalse(exist);
            IEnumerable<Guid> ids = orders.Select(f => f.Id);
            bool existOrder = connection1.Query<Order>().Where(f => ids.Contains(f.Id)).Any();
            Assert.IsFalse(existOrder);
        }

        /// <summary>
        /// 写入单条测试
        /// </summary>

        [TestMethod]
        public void InsertOneTest()
        {
            using IDbConnection connection = CreateConnection();
            Buyer buyer = CreateBuyer();
            connection.Insert(buyer);
            Order insertEntity = CreateOrders(1, 1, buyer).First();
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
            Buyer buyer = CreateBuyer();
            connection.Insert(buyer);
            IList<Order> orders = CreateOrders(100, 10, buyer).ToList();
            int values = connection.Insert(orders);
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
                using IDbConnection connection = CreateConnection();
                connection.Insert(buyer);
                return connection.Insert(orders);
            });
            using IDbConnection connection1 = CreateConnection();
            bool exist = connection1.Query<Buyer>().Where(f => f.Id == buyer.Id).Any();
            Assert.IsTrue(exist);
            IEnumerable<Guid> ids = orders.Select(f => f.Id);
            bool existOrder = connection1.Query<Order>().Where(f => ids.Contains(f.Id)).Any();
            Assert.IsTrue(existOrder);
        }

        /// <summary>
        /// 事务写入多条部分失败测试
        /// </summary>
        [TestMethod]
        public void InsertManyPartFailUseDbTransactionTest()
        {
            using IDbConnection connection = CreateConnection();
            connection.Open();
            IDbTransaction transaction = connection.BeginTransaction();
            Buyer buyer = CreateBuyer();
            IList<Order> orders = CreateOrders(100, 100, buyer).ToList();
            try
            {
                connection.Insert(buyer, transaction);
                var values = connection.InsertBulk(orders, transaction);
                transaction.Commit();
                Assert.IsTrue(values > 0);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                transaction.Rollback();
            }
            bool exist = connection.Query<Buyer>().Where(f => f.Id == buyer.Id).Any();
            Assert.IsFalse(exist);
            IEnumerable<Guid> ids = orders.Select(f => f.Id);
            bool existOrder = connection.Query<Order>().Where(f => ids.Contains(f.Id)).Any();
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
                using IDbConnection connection = CreateConnection();
                using TransactionScope trans = new TransactionScope();
                connection.Insert(buyer);
                connection.Insert(orders);
                trans.Complete();
            });
            using IDbConnection connection1 = CreateConnection();
            bool exist = connection1.Query<Buyer>().Where(f => f.Id == buyer.Id).Any();
            Assert.IsFalse(exist);
            IEnumerable<Guid> ids = orders.Select(f => f.Id);
            bool existOrder = connection1.Query<Order>().Where(f => ids.Contains(f.Id)).Any();
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
            using IDbConnection connection = CreateConnection();
            connection.Open();
            IDbTransaction transaction = connection.BeginTransaction();
            try
            {
                connection.Insert(buyer, transaction);
                connection.Insert(orders, transaction);
                connection.Insert(items, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
            }
            bool exist = connection.Query<Buyer>().Where(f => f.Id == buyer.Id).Any();
            Assert.IsTrue(exist);
            IEnumerable<Guid> orderIds = orders.Select(f => f.Id);
            int count = connection.Query<Order>().Where(f => orderIds.Contains(f.Id)).Count();
            Assert.AreEqual(orders.Count, count);
            IEnumerable<Guid> itemIds = items.Select(f => f.Id);
            count = connection.Query<Item>().Where(f => itemIds.Contains(f.Id)).Count();
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
                using IDbConnection connection = CreateConnection();
                connection.Insert(buyer);
                connection.Insert(orders);
                connection.Insert(items);
                trans.Complete();
            }
            using IDbConnection connection1 = CreateConnection();
            bool exist = connection1.Query<Buyer>().Where(f => f.Id == buyer.Id).Any();
            Assert.IsTrue(exist);
            IEnumerable<Guid> orderIds = orders.Select(f => f.Id);
            int count = connection1.Query<Order>().Where(f => orderIds.Contains(f.Id)).Count();
            Assert.AreEqual(orders.Count, count);
            IEnumerable<Guid> itemIds = items.Select(f => f.Id);
            count = connection1.Query<Item>().Where(f => itemIds.Contains(f.Id)).Count();
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
            using IDbConnection connection = CreateConnection();
            connection.Open();
            IDbTransaction transaction = connection.BeginTransaction();
            try
            {
                connection.Insert(buyer, transaction);
                connection.InsertBulk(orders, transaction);
                connection.InsertBulk(items, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
            }
            bool exist = connection.Query<Buyer>().Where(f => f.Id == buyer.Id).Any();
            Assert.IsTrue(exist);
            IEnumerable<Guid> orderIds = orders.Select(f => f.Id);
            int count = connection.Query<Order>().Where(f => orderIds.Contains(f.Id)).Count();
            Assert.AreEqual(orders.Count, count);
            IEnumerable<Guid> itemIds = items.Select(f => f.Id);
            count = connection.Query<Item>().Where(f => itemIds.Contains(f.Id)).Count();
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
                using IDbConnection connection = CreateConnection();
                connection.Insert(buyer);
                connection.InsertBulk(orders);
                connection.InsertBulk(items);
                trans.Complete();
            }
            using IDbConnection connection1 = CreateConnection();
            bool exist = connection1.Query<Buyer>().Where(f => f.Id == buyer.Id).Any();
            Assert.IsTrue(exist);
            IEnumerable<Guid> orderIds = orders.Select(f => f.Id);
            int count = connection1.Query<Order>().Where(f => orderIds.Contains(f.Id)).Count();
            Assert.AreEqual(orders.Count, count);
            IEnumerable<Guid> itemIds = items.Select(f => f.Id);
            count = connection1.Query<Item>().Where(f => itemIds.Contains(f.Id)).Count();
            Assert.AreEqual(items.Count, count);
        }

        /// <summary>
        /// 异步写入测试
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task InsertOneAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            Buyer buyer = CreateBuyer();
            int result = await connection.InsertAsync(buyer);
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
            IEnumerable<Buyer> buyers = Enumerable.Range(0, 100).Select(f => CreateBuyer());
            int result = await connection.InsertAsync(buyers);
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
            IList<Buyer> buyers = Enumerable.Range(0, 100).Select(f => CreateBuyer()).ToList();
            int result = await connection.InsertBulkAsync(buyers);
            Assert.IsTrue(result > 0);
        }
    }
}