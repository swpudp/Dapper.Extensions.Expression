using Dapper.Extensions.Expression.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Extensions.Expression.UnitTests.Dm
{
    [TestClass]
    public class NamingPolicyTests : DmBaseTest
    {
        [TestInitialize]
        public void Initialize()
        {
            NamingUtils.SetNamingPolicy(NamingPolicy.UpperSnakeCase);
        }

        /// <summary>
        /// 获取SnakeCase表名测试
        /// </summary>
        [TestMethod]
        public void GetSnakeCaseTableNameTest()
        {
            using IDbConnection connection = CreateConnection();
            string tableName = connection.GetTableName<NamingPolicySnakeCase>();
            Assert.AreEqual("\"NAMING_POLICY_SNAKE_CASE\"", tableName);
        }

        /// <summary>
        /// SnakeCase写入测试
        /// </summary>
        [TestMethod]
        public async Task SnakeCaseInsertTest()
        {
            using IDbConnection connection = CreateConnection();
            System.Collections.Generic.IList<NamingPolicySnakeCase> data = ObjectUtils.CreateNamingPolicyTestList(100, NamingPolicy.SnakeCase).AsList();
            int count = await connection.InsertBulkAsync(data, null);
            Assert.AreEqual(100, count);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public void UpdateTest()
        {
            using IDbConnection connection = CreateConnection();
            NamingPolicySnakeCase snakeCase = connection.Get<NamingPolicySnakeCase>(f => f.Id == new Guid("4b78b9e2-d2d8-48a4-887f-1ffa39c63327"));
            Assert.IsNotNull(snakeCase);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public async Task UpdateAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            NamingPolicySnakeCase entity = connection.Get<NamingPolicySnakeCase>(f => f.Id == new Guid("6a32c4a1-ddb9-44dc-9fa5-4adf5fddadec"));
            Assert.IsNotNull(entity);
            entity.CreateTime = DateTime.Now;
            int updated = await connection.UpdateAsync(entity);
            Assert.IsTrue(updated > 0);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public void UpdateObjectTest()
        {
            string id = "4b78b9e2-d2d8-48a4-887f-1ffa39c63327";
            using IDbConnection connection = CreateConnection();
            decimal amount = Convert.ToDecimal(new Random().NextDouble() * 100);
            int updated = connection.Update<NamingPolicySnakeCase>(f => f.Id == Guid.Parse(id), f => new { CreateTime = DateTime.Now });
            Assert.IsTrue(updated > 0);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public async Task UpdateObjectAsyncTest()
        {
            string id = "4b78b9e2-d2d8-48a4-887f-1ffa39c63327";
            using IDbConnection connection = CreateConnection();
            decimal amount = Convert.ToDecimal(new Random().NextDouble() * 100);
            int updated = await connection.UpdateAsync<NamingPolicySnakeCase>(f => f.Id == Guid.Parse(id), f => new
            {
                NamingType = NamingPolicy.SnakeCase,
                CreateTime = DateTime.Now
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
            string id = "207a59de-ec80-4411-85b6-6da285641714";
            int updated = connection.Update<NamingPolicySnakeCase>(f => f.Id == Guid.Parse(id), f => new NamingPolicySnakeCase
            {
                CreateTime = DateTime.Now,
                NamingType = NamingPolicy.UpperCase,
                Version = f.Version + 1
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
            Guid id = Guid.Parse("207a59de-ec80-4411-85b6-6da285641714");
            int updated = connection.Update<NamingPolicySnakeCase>(f => f.Id == id, f => new NamingPolicySnakeCase { CreateTime = DateTime.Now });
            Assert.IsTrue(updated > 0);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public async Task UpdateEntityAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            string id = "446b2e5d-63ae-4fda-aef4-2d40e9f12356";
            int updated = await connection.UpdateAsync<NamingPolicySnakeCase>(f => f.Id == Guid.Parse(id), f => new NamingPolicySnakeCase { CreateTime = DateTime.Now });
            Assert.IsTrue(updated > 0);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public async Task UpdateEntityByIdAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            Guid id = Guid.Parse("446b2e5d-63ae-4fda-aef4-2d40e9f12356");
            int updated = await connection.UpdateAsync<NamingPolicySnakeCase>(f => f.Id == id, f => new NamingPolicySnakeCase { CreateTime = DateTime.Now });
            Assert.IsTrue(updated > 0);
        }

        /// <summary>
        /// 更新测试
        /// </summary>
        [TestMethod]
        public async Task UpdateUnaryAsyncTest()
        {
            using IDbConnection connection = CreateConnection();
            Guid id = Guid.Parse("446b2e5d-63ae-4fda-aef4-2d40e9f12356");
            NamingPolicySnakeCase entity = connection.Get<NamingPolicySnakeCase>(f => f.Id == id);
            Assert.IsNotNull(entity);
            int updated = await connection.UpdateAsync<NamingPolicySnakeCase>(f => f.Id == id && f.Version == entity.Version, f => new NamingPolicySnakeCase
            {
                CreateTime = DateTime.Now,
                NamingType = NamingPolicy.LowerCase,
                Version = f.Version + 1
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
            Guid id = Guid.Parse("446b2e5d-63ae-4fda-aef4-2d40e9f12356");
            NamingPolicySnakeCase entity = connection.Get<NamingPolicySnakeCase>(f => f.Id == id);
            Assert.IsNotNull(entity);
            int updated = await connection.UpdateAsync<NamingPolicySnakeCase>(f => f.Id == id && f.Version == entity.Version, f => new NamingPolicySnakeCase
            {
                CreateTime = DateTime.Now,
                NamingType = NamingPolicy.UpperCase,
                Version = f.Version - 1
            });
            Assert.IsTrue(updated > 0);
        }
    }
}