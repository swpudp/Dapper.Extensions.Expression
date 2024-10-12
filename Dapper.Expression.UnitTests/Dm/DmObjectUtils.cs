using System;
using System.Collections.Generic;
using System.Linq;

namespace Dapper.Extensions.Expression.UnitTests.Dm
{
    internal static class DmObjectUtils
    {
        public static Buyer CreateBuyer()
        {
            Buyer buyer = new Buyer
            {
                Id = Guid.NewGuid(),
                Code = CommonTestUtils.GetRandomString(2),
                CreateTime = DateTime.Now,
                Email = CommonTestUtils.GetRandomString(4) + "@" + CommonTestUtils.GetRandomString(2) + ".com",
                Identity = CommonTestUtils.GetRandomString(10),
                IsActive = true,
                IsDelete = false,
                Mobile = "13900000000",
                Name = CommonTestUtils.GetRandomString(4),
                Type = BuyerType.Company,

            };
            return buyer;
        }

        public static IEnumerable<Item> CreateItems(IEnumerable<Order> orders)
        {
            foreach (Order order in orders)
            {
                for (int i = 0; i < 10; i++)
                {
                    yield return new Item
                    {
                        Amount = Convert.ToDecimal(Math.Sin(i) * 100),
                        Code = CommonTestUtils.GetRandomString(8),
                        Discount = Convert.ToDecimal(Math.Sin(i) * 10),
                        Id = Guid.NewGuid(),
                        Index = i,
                        Name = CommonTestUtils.GetRandomString(20),
                        OrderId = order.Id,
                        Price = Convert.ToDecimal(Math.Sin(i) * 90),
                        Quantity = Convert.ToDecimal(Math.Sin(i) * 80),
                        Unit = CommonTestUtils.GetRandomString(2),
                        Version = i
                    };
                }
            }
        }
        public static IEnumerable<Attachment> CreateAttachments(IEnumerable<Order> orders)
        {
            foreach (Order order in orders)
            {
                int total = CommonTestUtils.GetRandomInt(1, 5);
                string ext = CommonTestUtils.GetRandomExt();
                for (int i = 0; i < total; i++)
                {
                    yield return new Attachment
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        Name = CommonTestUtils.GetRandomString(8) + ext,
                        Version = i,
                        Extend = ext,
                        Enable = i
                    };
                }
            }
        }
        public static IEnumerable<Order> CreateOrders(int count, int max, Buyer buyer)
        {
            return Enumerable.Range(0, count).Select((f, index) => new Order
            {
                Id = Guid.NewGuid(),
                Amount = Convert.ToDecimal(Math.Sin(index)) * 500,
                BuyerId = buyer.Id,
                CreateTime = DateTime.Now,
                Freight = index % 5 == 0 ? Convert.ToDecimal(Math.Sin(index)) * 20 : default(decimal?),
                DocId = index % 5 == 0 ? Guid.NewGuid() : default(Guid?),
                Ignore = CommonTestUtils.GetRandomString(10),
                IsActive = index % 50 == 0,
                IsDelete = index % 60 == 0,
                SerialNo = CommonTestUtils.GetRandomString((index + 1) % max),
                Remark = CommonTestUtils.GetRandomString((index + 1) % 50),
                Status = (Status)(index % 3),
                SignState = index % 10 == 0 ? (SignState)(index % 2) : default(SignState?),
                Version = index,
                Index = index
            }).ToList();
        }

        public static IEnumerable<NamingPolicySnakeCase> CreateNamingPolicyTestList(int count, NamingPolicy namingPolicy)
        {
            return Enumerable.Range(0, count).Select((f, index) => new NamingPolicySnakeCase
            {
                Id = Guid.NewGuid(),
                NamingType = namingPolicy,
                CreateTime = DateTime.Now,
                Version = index
            }).ToList();
        }

        public static IEnumerable<Log> CreateLogs(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new Log()
                {
                    Id = Guid.NewGuid(),
                    Version = i,
                    LogType = (LogType)(i % 2),
                    Logged = DateTime.Now
                };
            }
        }
    }
}
