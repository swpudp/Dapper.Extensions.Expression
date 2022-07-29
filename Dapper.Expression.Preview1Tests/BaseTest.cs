using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extensions.Expression.UnitTests
{
    public abstract class BaseTest
    {
        protected static IDbConnection CreateConnection()
        {
            IDbConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;database=dapper_exp;uid=root;pwd=Q1@we34r;charset=utf8");
            return connection;
        }

        protected static async Task Execute(Func<IDbConnection, Task> action)
        {
            using (IDbConnection connection = CreateConnection())
            {
                await action(connection);
            }
        }

        protected static void Execute(Action<IDbConnection> action)
        {
            using (IDbConnection connection = CreateConnection())
            {
                action(connection);
            }
        }

        protected static async Task<T> Execute<T>(Func<IDbConnection, Task<T>> action)
        {
            using (IDbConnection connection = CreateConnection())
            {
                return await action(connection);
            }
        }

        protected static T Execute<T>(Func<IDbConnection, T> action)
        {
            using (IDbConnection connection = CreateConnection())
            {
                return action(connection);
            }
        }

        protected static int ExecuteTransaction(Func<IDbConnection, IDbTransaction, IEnumerable<int>> actions)
        {
            using (IDbConnection connection = CreateConnection())
            {
                connection.Open();
                IDbTransaction transaction = connection.BeginTransaction();
                try
                {
                    IEnumerable<int> total = actions(connection, transaction);
                    transaction.Commit();
                    return total.Sum();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        protected static string GetRandomString(int length)
        {
            byte[] bytes = new byte[length];
            Span<byte> span = new Span<byte>(bytes, 0, bytes.Length);
            RandomNumberGenerator.Fill(span);
            StringBuilder builder = new StringBuilder();
            foreach (byte b in span)
            {
                builder.AppendFormat("{0:X2}", b);
            }
            return builder.ToString();
        }

        protected static Buyer CreateBuyer()
        {
            Buyer buyer = new Buyer
            {
                Id = Guid.NewGuid(),
                Code = GetRandomString(2),
                CreateTime = DateTime.Now,
                Email = GetRandomString(4) + "@" + GetRandomString(2) + ".com",
                Identity = GetRandomString(10),
                IsActive = true,
                IsDelete = false,
                Mobile = "13900000000",
                Name = GetRandomString(4),
                Type = BuyerType.Company,

            };
            return buyer;
        }

        protected static IEnumerable<Item> CreateItems(IEnumerable<Order> orders)
        {
            foreach (Order order in orders)
            {
                for (int i = 0; i < 10; i++)
                {
                    yield return new Item
                    {
                        Amount = Convert.ToDecimal(Math.Sin(i) * 100),
                        Code = GetRandomString(8),
                        Discount = Convert.ToDecimal(Math.Sin(i) * 10),
                        Id = Guid.NewGuid(),
                        Index = i,
                        Name = GetRandomString(20),
                        OrderId = order.Id,
                        Price = Convert.ToDecimal(Math.Sin(i) * 90),
                        Quantity = Convert.ToDecimal(Math.Sin(i) * 80),
                        Unit = GetRandomString(2),
                        Version = i
                    };
                }
            }
        }

        private static Random _random = new Random();

        private static string[] extensions = { ".pdf", ".doc", ".gif", ".jpg", ".xls" };

        protected static IEnumerable<Attachment> CreateAttachments(IEnumerable<Order> orders)
        {
            foreach (Order order in orders)
            {
                int total = _random.Next(1, 5);
                string ext = extensions[_random.Next(extensions.Length)];
                for (int i = 0; i < total; i++)
                {
                    yield return new Attachment
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        Name = GetRandomString(8) + ext,
                        Version = i,
                        Extend = ext
                    };
                }
            }
        }

        protected static IEnumerable<Order> CreateOrders(int count, int max, Buyer buyer)
        {
            return Enumerable.Range(0, count).Select((f, index) => new Order
            {
                Id = Guid.NewGuid(),
                Amount = Convert.ToDecimal(Math.Sin(index)) * 500,
                BuyerId = buyer.Id,
                CreateTime = DateTime.Now,
                Freight = index % 5 == 0 ? Convert.ToDecimal(Math.Sin(index)) * 20 : default(decimal?),
                DocId = index % 5 == 0 ? Guid.NewGuid() : default(Guid?),
                Ignore = GetRandomString(10),
                IsActive = index % 50 == 0,
                IsDelete = index % 60 == 0,
                SerialNo = GetRandomString((index + 1) % max),
                Remark = GetRandomString((index + 1) % 50),
                Status = (Status)(index % 3),
                SignState = index % 10 == 0 ? (SignState)(index % 2) : default(SignState?),
                Version = index,
                Index = index
            }).ToList();
        }

        protected IEnumerable<Log> CreateLogs(int count)
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

        protected void Test()
        {
            Trace.WriteLine(nameof(Test) + "16点58分");
            Trace.WriteLine(nameof(Test) + "17点04分");
            Trace.WriteLine(nameof(Test) + "17点38分");
            Trace.WriteLine(nameof(Test) + "17点40分");
            Trace.WriteLine(nameof(Test) + "17点41分");
        }
    }
}
