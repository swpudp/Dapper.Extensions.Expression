using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Extensions.Expression.UnitTests
{
    public abstract class BaseTest
    {
        protected abstract IDbConnection CreateConnection();

        protected async Task Execute(Func<IDbConnection, Task> action)
        {
            using (IDbConnection connection = CreateConnection())
            {
                await action(connection);
            }
        }

        protected void Execute(Action<IDbConnection> action)
        {
            using (IDbConnection connection = CreateConnection())
            {
                action(connection);
            }
        }

        protected async Task<T> Execute<T>(Func<IDbConnection, Task<T>> action)
        {
            using (IDbConnection connection = CreateConnection())
            {
                return await action(connection);
            }
        }

        protected T Execute<T>(Func<IDbConnection, T> action)
        {
            using (IDbConnection connection = CreateConnection())
            {
                return action(connection);
            }
        }

        protected int ExecuteTransaction(Func<IDbConnection, IDbTransaction, IEnumerable<int>> actions)
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
