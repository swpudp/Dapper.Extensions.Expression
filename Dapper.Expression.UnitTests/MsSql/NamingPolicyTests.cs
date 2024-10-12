using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Extensions.Expression.UnitTests.MsSql
{
    [TestClass]
    public class NamingPolicyTests : MsSqlBaseTest
    {
        /// <summary>
        /// 获取SnakeCase表名测试
        /// </summary>
        [TestMethod]
        public void GetSnakeCaseTableNameTest()
        {
            using IDbConnection connection = CreateConnection();
            string tableName = connection.GetTableName<NamingPolicySnakeCase>();
            Assert.AreEqual("[naming_policy_snake_case]", tableName);
        }

        /// <summary>
        /// SnakeCase写入测试
        /// </summary>
        [TestMethod]
        public async Task SnakeCaseInsertTest()
        {
            using IDbConnection connection = CreateConnection();
            System.Collections.Generic.IList<NamingPolicySnakeCase> data = MsSqlObjectUtils.CreateNamingPolicyTestList(100, NamingPolicy.SnakeCase).AsList();
            int count = await connection.InsertBulkAsync<NamingPolicySnakeCase>(data, null);
            Assert.AreEqual(100, count);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public void UpdateTest()
        {
            using IDbConnection connection = CreateConnection();
            NamingPolicySnakeCase snakeCase = connection.Get<NamingPolicySnakeCase>(f => f.Id == new Guid("b6809cff-84b3-4591-b49c-00b42ab9c736"));
            Assert.IsNotNull(snakeCase);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public async Task UpdateAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            Order order = connection.Get<Order>(f => f.Id == new Guid("7e198311-0bdc-4dec-97a1-01b03c356eba"));
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
            string id = "6ae8d4f8-b340-48ba-b768-00216aecbd7f";
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
            string id = "ecdaf6a3-91de-4ec8-8f15-018302e2284b";
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
            string id = "7e198311-0bdc-4dec-97a1-01b03c356eba";
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
            Guid id = Guid.Parse("2bcba2b6-6a84-4b77-90f6-01e477d8594d");
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
            string id = "4e9395b0-56eb-41b6-95a4-020dc083014d";
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
            Guid id = Guid.Parse("36783f7f-1e22-4d24-b592-029a5775e21d");
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
            Guid id = Guid.Parse("93da3349-e0ea-4526-8967-09fef1b7ba83");
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
            Guid id = Guid.Parse("ac390ec8-a450-46e8-92f6-029b00004b94");
            Order order = connection.Get<Order>(f => f.Id == id);
            Assert.IsNotNull(order);
            int updated = await connection.UpdateAsync<Order>(f => f.Id == id && f.Version == order.Version, f => new Order
            {
                SerialNo = CommonTestUtils.GetRandomString(10),
                IsDelete = true,
                IsActive = id != Guid.Empty,
                Version = f.Version - 1
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
            Guid id = Guid.Parse("66a3d047-74ee-4841-8267-02d192a4a0c8");
            Order order = connection.Get<Order>(f => f.Id == id);
            Assert.IsNotNull(order);
            int updated = await connection.UpdateAsync<Order>(f => f.Id == id && f.IsEnable && !f.IsDelete && !f.IsActive.Value, f => new Order
            {
                SerialNo = CommonTestUtils.GetRandomString(10),
                IsDelete = false,
                IsActive = id != Guid.Empty,
                Version = f.Version - 1,
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
            Guid id = Guid.Parse("d88d875f-d4f8-414d-aefc-033e66eb0a2a");
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
            Guid id = Guid.Parse("06788aba-1731-4b35-a781-033fc7b8d162");
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