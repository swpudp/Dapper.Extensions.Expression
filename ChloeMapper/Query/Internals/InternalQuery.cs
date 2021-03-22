using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Extensions.Expression.Core;
using Dapper.Extensions.Expression.Data;
using Dapper.Extensions.Expression.Extensions;
using Dapper.Extensions.Expression.Infrastructure;
using Dapper.Extensions.Expression.Mapper;
using Dapper.Extensions.Expression.Mapper.Activators;
using Dapper.Extensions.Expression.Query.Mapping;
using Dapper.Extensions.Expression.Query.QueryState;
using Dapper.Extensions.Expression.Query.Visitors;
using Dapper.Extensions.Expression.Reflection;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Extensions.Expression.Query.Internals
{
    class InternalQuery<T> : IEnumerable<T>, IEnumerable
    {
        Query<T> _query;

        internal InternalQuery(Query<T> query)
        {
            this._query = query;
        }

        DbCommandFactor GenerateCommandFactor()
        {
            IQueryState qs = QueryExpressionResolver.Resolve(this._query.QueryExpression, new ScopeParameterDictionary(), new StringSet());
            MappingData data = qs.GenerateMappingData();

            IObjectActivator objectActivator;
            if (this._query._trackEntity)
                objectActivator = data.ObjectActivatorCreator.CreateObjectActivator(this._query.DbContext);
            else
                objectActivator = data.ObjectActivatorCreator.CreateObjectActivator();

            IDbExpressionTranslator translator = this._query.DbContext.DatabaseProvider.CreateDbExpressionTranslator();
            DbCommandInfo dbCommandInfo = translator.Translate(data.SqlQuery);

            DbCommandFactor commandFactor = new DbCommandFactor(objectActivator, dbCommandInfo.CommandText, dbCommandInfo.GetParameters());
            return commandFactor;
        }

        public IEnumerator<T> GetEnumerator()
        {
            DbCommandFactor commandFactor = this.GenerateCommandFactor();
            QueryEnumerator<T> enumerator = QueryEnumeratorCreator.CreateEnumerator<T>(commandFactor, async (cmdFactor, @async) =>
            {
                IDataReader dataReader = await this._query.DbContext.Session.ExecuteReader(cmdFactor.CommandText, CommandType.Text, cmdFactor.Parameters, @async);

                return DataReaderReady(dataReader, cmdFactor.ObjectActivator);
            });
            return enumerator;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public List<T> Execute()
        {
            return this.ToList();
        }
        public async Task<List<T>> ExecuteAsync()
        {
            IAsyncEnumerator<T> enumerator = this.GetEnumerator() as IAsyncEnumerator<T>;

            List<T> list = new List<T>();
            using (enumerator)
            {
                while (await enumerator.MoveNextAsync())
                {
                    list.Add(enumerator.Current);
                }
            }

            return list;
        }

        public override string ToString()
        {
            DbCommandFactor commandFactor = this.GenerateCommandFactor();
            return AppendDbCommandInfo(commandFactor.CommandText, commandFactor.Parameters);
        }

        static IDataReader DataReaderReady(IDataReader dataReader, IObjectActivator objectActivator)
        {
            if (objectActivator is RootEntityActivator)
            {
                dataReader = new QueryDataReader(dataReader);
            }

            objectActivator.Prepare(dataReader);

            return dataReader;
        }

        static string AppendDbCommandInfo(string cmdText, DbParam[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            if (parameters != null)
            {
                foreach (DbParam param in parameters)
                {
                    if (param == null)
                        continue;

                    string typeName = null;
                    object value = null;
                    Type parameterType;
                    if (param.Value == null || param.Value == DBNull.Value)
                    {
                        parameterType = param.Type;
                        value = "NULL";
                    }
                    else
                    {
                        value = param.Value;
                        parameterType = param.Value.GetType();

                        if (parameterType == typeof(string) || parameterType == typeof(DateTime))
                            value = "'" + value + "'";
                    }

                    if (parameterType != null)
                        typeName = GetTypeName(parameterType);

                    sb.AppendFormat("{0} {1} = {2};", typeName, param.Name, value);
                    sb.AppendLine();
                }
            }

            sb.AppendLine(cmdText);

            return sb.ToString();
        }
        static string GetTypeName(Type type)
        {
            Type underlyingType;
            if (ReflectionExtension.IsNullable(type, out underlyingType))
            {
                return string.Format("Nullable<{0}>", GetTypeName(underlyingType));
            }

            return type.Name;
        }
    }
}
