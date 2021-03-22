
using System;
using System.Collections;
using System.Threading.Tasks;

namespace Dapper.Extensions.Expression.Core
{
    internal interface IAsyncEnumerator : IEnumerator
    {
        Task<bool> MoveNextAsync();
    }
    internal interface IAsyncEnumerator<out T> : IAsyncEnumerator, IDisposable
    {
        new T Current { get; }
    }
}
