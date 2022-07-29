using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace EfTest
{
    [TestClass]
    public class QueryTest
    {
        [TestMethod]
        public void SumTest()
        {
            using (TestDbContext ctx = new TestDbContext())
            {
                var result = ctx.Orders.Where(f => f.Status == Status.Running && f.SerialNo == "11111111").Sum(x => x.Version);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public void GroupTest()
        {
            using (TestDbContext ctx = new TestDbContext())
            {
                var result = ctx.Orders.Where(f => f.Status == Status.Running)
                    .GroupBy(f => f.SerialNo)
                    .Select(x => new { Version = x.Key, Max = x.Max() });
                Assert.IsNotNull(result);
            }
        }
    }
}
