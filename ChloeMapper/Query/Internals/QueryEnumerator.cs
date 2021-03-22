using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper.Extensions.Expression.Core;
using Dapper.Extensions.Expression.Data;
using Dapper.Extensions.Expression.Extensions;
using Dapper.Extensions.Expression.Mapper;

namespace Dapper.Extensions.Expression.Query.Internals
{
    internal static class QueryEnumeratorCreator
    {
        public static QueryEnumerator<T> CreateEnumerator<T>(DbCommandFactor commandFactor, Func<DbCommandFactor, bool, Task<IDataReader>> dataReaderGetter)
        {
            return new QueryEnumerator<T>(commandFactor, dataReaderGetter);
        }
    }
    internal class QueryEnumerator<T> : IEnumerator<T>, IAsyncEnumerator<T>
    {
        DbCommandFactor _commandFactor;
        Func<DbCommandFactor, bool, Task<IDataReader>> _dataReaderGetter;
        IObjectActivator _objectActivator;

        IDataReader _reader;

        T _current;
        bool _hasFinished;
        bool _disposed;
        public QueryEnumerator(DbCommandFactor commandFactor, Func<DbCommandFactor, bool, Task<IDataReader>> dataReaderGetter)
        {
            this._commandFactor = commandFactor;
            this._dataReaderGetter = dataReaderGetter;
            this._objectActivator = commandFactor.ObjectActivator;

            this._reader = null;

            this._current = default(T);
            this._hasFinished = false;
            this._disposed = false;
        }

        public T Current { get { return this._current; } }

        object IEnumerator.Current { get { return this._current; } }

        public bool MoveNext()
        {
            return this.MoveNext(false).GetResult();
        }

        public Task<bool> MoveNextAsync()
        {
            return this.MoveNext(true);
        }

        async Task<bool> MoveNext(bool @async)
        {
            if (this._hasFinished || this._disposed)
                return false;

            if (this._reader == null)
            {
                //TODO 执行 sql 语句，获取 DataReader
                this._reader = await this._dataReaderGetter(this._commandFactor, @async);
            }

            bool readResult = @async ? await this._reader.Read(@async) : this._reader.Read();
            if (readResult)
            {
                if (@async)
                    this._current = (T)(await this._objectActivator.CreateInstanceAsync(this._reader));
                else
                    this._current = (T)this._objectActivator.CreateInstance(this._reader);

                return true;
            }
            else
            {
                this._reader.Close();
                this._current = default(T);
                this._hasFinished = true;
                return false;
            }
        }

        public void Dispose()
        {
            if (this._disposed)
                return;

            if (this._reader != null)
            {
                if (!this._reader.IsClosed)
                    this._reader.Close();
                this._reader.Dispose();
                this._reader = null;
            }

            if (!this._hasFinished)
            {
                this._hasFinished = true;
            }

            this._current = default(T);
            this._disposed = true;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }
    }
}
