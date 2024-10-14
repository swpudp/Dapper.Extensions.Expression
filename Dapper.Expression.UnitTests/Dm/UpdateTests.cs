using Dapper.Extensions.Expression.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Extensions.Expression.UnitTests.Dm
{
    [TestClass]
    public class UpdateTests : DmBaseTest
    {
        [TestInitialize]
        public void Initialize()
        {
            NamingUtils.SetNamingPolicy(NamingPolicy.UpperCase);
        }

        /// <summary>
        /// 获取表名测试
        /// </summary>
        [TestMethod]
        public void GetTableNameTest()
        {
            using IDbConnection connection = CreateConnection();
            string tableName = connection.GetTableName<Order>();
            Assert.AreEqual("\"ORDER\"", tableName);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public void UpdateTest()
        {
            using IDbConnection connection = CreateConnection();
            Order order = connection.Get<Order>(f => f.Id == new Guid("e5da4e4c-550f-4cf1-89e6-ac4d9a1e26ff"));
            Assert.IsNotNull(order);
            order.UpdateTime = DateTime.Now;
            order.Amount = Convert.ToDecimal(new Random().NextDouble() * 100);
            int updated = connection.Update(order);
            Assert.IsTrue(updated > 0);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public async Task UpdateAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            Order order = connection.Get<Order>(f => f.Id == new Guid("228f699d-d692-4721-a0b9-6d835c872c42"));
            Assert.IsNotNull(order);
            order.UpdateTime = DateTime.Now;
            order.Amount = Convert.ToDecimal(new Random().NextDouble() * 90);
            int updated = await connection.UpdateAsync(order);
            Assert.IsTrue(updated > 0);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public void UpdateObjectTest()
        {
            string id = "e5da4e4c-550f-4cf1-89e6-ac4d9a1e26ff";
            using IDbConnection connection = CreateConnection();
            decimal amount = Convert.ToDecimal(new Random().NextDouble() * 100);
            int updated = connection.Update<Order>(f => f.Id == Guid.Parse(id), f => new { UpdateTime = DateTime.Now, Amount = amount });
            Assert.IsTrue(updated > 0);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public async Task UpdateObjectAsyncTest()
        {
            string id = "e5da4e4c-550f-4cf1-89e6-ac4d9a1e26ff";
            using IDbConnection connection = CreateConnection();
            decimal amount = Convert.ToDecimal(new Random().NextDouble() * 100);
            int updated = await connection.UpdateAsync<Order>(f => f.Id == Guid.Parse(id), f => new
            {
                UpdateTime = DateTime.Now,
                Amount = amount,
                Number = CommonTestUtils.GetRandomString(10),
                IsDelete = true,
                IsActive = f.IsDelete,
                Version = f.Version + 1,
                Remark = f.SerialNo
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
            string id = "e5da4e4c-550f-4cf1-89e6-ac4d9a1e26ff";
            int updated = connection.Update<Order>(f => f.Id == Guid.Parse(id), f => new Order
            {
                SerialNo = CommonTestUtils.GetRandomString(10),
                IsDelete = true,
                IsActive = f.IsDelete,
                Version = f.Version + 1,
                Remark = f.SerialNo
            });
            Assert.IsTrue(updated > 0);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public void UpdateEntityByIdTest()
        {
            using IDbConnection connection = CreateConnection();
            Guid id = Guid.Parse("e5da4e4c-550f-4cf1-89e6-ac4d9a1e26ff");
            int updated = connection.Update<Order>(f => f.Id == id, f => new Order { SerialNo = CommonTestUtils.GetRandomString(10), IsDelete = true });
            Assert.IsTrue(updated > 0);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public async Task UpdateEntityAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            string id = "e5da4e4c-550f-4cf1-89e6-ac4d9a1e26ff";
            int updated = await connection.UpdateAsync<Order>(f => f.Id == Guid.Parse(id), f => new Order { SerialNo = CommonTestUtils.GetRandomString(10), IsDelete = true });
            Assert.IsTrue(updated > 0);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public async Task UpdateEntityByIdAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            Guid id = Guid.Parse("e5da4e4c-550f-4cf1-89e6-ac4d9a1e26ff");
            int updated = await connection.UpdateAsync<Order>(f => f.Id == id, f => new Order { SerialNo = CommonTestUtils.GetRandomString(10), IsDelete = true });
            Assert.IsTrue(updated > 0);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public async Task UpdateUnaryAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            Guid id = Guid.Parse("e5da4e4c-550f-4cf1-89e6-ac4d9a1e26ff");
            Order order = connection.Get<Order>(f => f.Id == id);
            Assert.IsNotNull(order);
            int updated = await connection.UpdateAsync<Order>(f => f.Id == id && f.Version == order.Version, f => new Order
            {
                SerialNo = CommonTestUtils.GetRandomString(10),
                IsDelete = true,
                Version = f.Version + 1,
                Amount = f.Amount * 2,
                Freight = f.Freight / 2
            });
            Assert.IsTrue(updated > 0);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public async Task UpdateBinaryNotEqualsAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            Guid id = Guid.Parse("e5da4e4c-550f-4cf1-89e6-ac4d9a1e26ff");
            Order order = connection.Get<Order>(f => f.Id == id);
            Assert.IsNotNull(order);
            int updated = await connection.UpdateAsync<Order>(f => f.Id == id && f.Version == order.Version, f => new Order
            {
                SerialNo = CommonTestUtils.GetRandomString(10),
                IsDelete = true,
                IsActive = id != Guid.Empty,
                Version = f.Version + 1
            });
            Assert.IsTrue(updated > 0);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public async Task UpdateNullableBooleanAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            Guid id = Guid.Parse("624cba44-5caf-4987-bb9a-35a9c2287b49");
            Order order = connection.Get<Order>(f => f.Id == id);
            Assert.IsNotNull(order);
            int updated = await connection.UpdateAsync<Order>(f => f.Id == id && f.IsDelete && !f.IsEnable && !f.IsActive.Value, f => new Order
            {
                SerialNo = CommonTestUtils.GetRandomString(10),
                Version = f.Version + 1,
                UpdateTime = DateTime.Now
            });
            Assert.IsTrue(updated > 0);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public async Task UpdateNullableGuidAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            Guid id = Guid.Parse("e5da4e4c-550f-4cf1-89e6-ac4d9a1e26ff");
            Order order = connection.Get<Order>(f => f.Id == id);
            Assert.IsNotNull(order);
            Guid docId = Guid.NewGuid();
            int updated = await connection.UpdateAsync<Order>(f => f.Id == id, f => new Order
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
        public async Task UpdateNullablePropertyAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            Guid id = Guid.Parse("e5da4e4c-550f-4cf1-89e6-ac4d9a1e26ff");
            Order order = connection.Get<Order>(f => f.Id == id);
            Assert.IsNotNull(order);
            order.DocId = null;
            Guid docId = Guid.NewGuid();
            int updated = await connection.UpdateAsync<Order>(f => f.DocId == order.DocId, f => new Order
            {
                Version = f.Version + 1,
                DocId = docId
            });
            Assert.IsTrue(updated > 0);
        }
    }
}