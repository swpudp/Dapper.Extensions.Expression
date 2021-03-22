using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dapper.Extensions.Expression.Annotations;
using Dapper.Extensions.Expression.Core;
using Dapper.Extensions.Expression.Data;
using Dapper.Extensions.Expression.Descriptors;
using Dapper.Extensions.Expression.Extensions;
using Dapper.Extensions.Expression.Infrastructure;
using Dapper.Extensions.Expression.Mapper;
using Dapper.Extensions.Expression.Mapper.Activators;
using Dapper.Extensions.Expression.Mapper.Binders;
using Dapper.Extensions.Expression.Query.Mapping;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Extensions.Expression.Query.Internals
{
    class InternalSqlQuery<T> : IEnumerable<T>, IEnumerable
    {
        DbContext _dbContext;
        string _sql;
        CommandType _cmdType;
        DbParam[] _parameters;

        public InternalSqlQuery(DbContext dbContext, string sql, CommandType cmdType, DbParam[] parameters)
        {
            this._dbContext = dbContext;
            this._sql = sql;
            this._cmdType = cmdType;
            this._parameters = parameters;
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

        public IEnumerator<T> GetEnumerator()
        {
            return new QueryEnumerator(this);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        class QueryEnumerator : IEnumerator<T>, IAsyncEnumerator<T>
        {
            InternalSqlQuery<T> _internalSqlQuery;

            IDataReader _reader;
            IObjectActivator _objectActivator;

            T _current;
            bool _hasFinished;
            bool _disposed;
            public QueryEnumerator(InternalSqlQuery<T> internalSqlQuery)
            {
                this._internalSqlQuery = internalSqlQuery;
                this._reader = null;
                this._objectActivator = null;

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
                    await this.Prepare(@async);
                }

                bool readResult = @async ? await this._reader.Read(@async) : this._reader.Read();
                if (readResult)
                {
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

            async Task Prepare(bool @async)
            {
                Type type = typeof(T);

                if (type != PublicConstants.TypeOfObject && MappingTypeSystem.IsMappingType(type))
                {
                    PrimitiveObjectActivatorCreator activatorCreator = new PrimitiveObjectActivatorCreator(type, 0);
                    this._objectActivator = activatorCreator.CreateObjectActivator();
                    this._reader = await this.ExecuteReader(@async);
                    return;
                }

                this._reader = await this.ExecuteReader(@async);
                this._objectActivator = GetObjectActivator(type, this._reader);
            }

            async Task<IDataReader> ExecuteReader(bool @async)
            {
                IDataReader reader = await this._internalSqlQuery._dbContext.AdoSession.ExecuteReader(this._internalSqlQuery._sql, this._internalSqlQuery._parameters, this._internalSqlQuery._cmdType, @async);

                return reader;
            }

            static IObjectActivator GetObjectActivator(Type type, IDataReader reader)
            {
                if (type == PublicConstants.TypeOfObject || type == typeof(DapperRow))
                {
                    return new DapperRowObjectActivator();
                }

                List<CacheInfo> caches;
                if (!ObjectActivatorCache.TryGetValue(type, out caches))
                {
                    if (!Monitor.TryEnter(type))
                    {
                        return CreateObjectActivator(type, reader);
                    }

                    try
                    {
                        caches = ObjectActivatorCache.GetOrAdd(type, new List<CacheInfo>(1));
                    }
                    finally
                    {
                        Monitor.Exit(type);
                    }
                }

                CacheInfo cache = TryGetCacheInfoFromList(caches, reader);

                if (cache == null)
                {
                    lock (caches)
                    {
                        cache = TryGetCacheInfoFromList(caches, reader);
                        if (cache == null)
                        {
                            ComplexObjectActivator activator = CreateObjectActivator(type, reader);
                            cache = new CacheInfo(activator, reader);
                            caches.Add(cache);
                        }
                    }
                }

                return cache.ObjectActivator;
            }
            static ComplexObjectActivator CreateObjectActivator(Type type, IDataReader reader)
            {
                ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                    throw new ArgumentException(string.Format("The type of '{0}' does't define a none parameter constructor.", type.FullName));

                ConstructorDescriptor constructorDescriptor = ConstructorDescriptor.GetInstance(constructor);
                ObjectMemberMapper mapper = constructorDescriptor.GetEntityMemberMapper();
                InstanceCreator instanceCreator = constructorDescriptor.GetInstanceCreator();
                List<IMemberBinder> memberBinders = PrepareMemberBinders(type, reader, mapper);

                ComplexObjectActivator objectActivator = new ComplexObjectActivator(instanceCreator, new List<IObjectActivator>(), memberBinders, null);
                objectActivator.Prepare(reader);

                return objectActivator;
            }
            static List<IMemberBinder> PrepareMemberBinders(Type type, IDataReader reader, ObjectMemberMapper mapper)
            {
                List<IMemberBinder> memberBinders = new List<IMemberBinder>(reader.FieldCount);

                MemberInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
                MemberInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetField);
                List<MemberInfo> members = new List<MemberInfo>(properties.Length + fields.Length);
                members.AddRange(properties);
                members.AddRange(fields);

                TypeDescriptor typeDescriptor = EntityTypeContainer.TryGetDescriptor(type);

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string name = reader.GetName(i);
                    MemberInfo mapMember = TryGetMapMember(members, name, typeDescriptor);

                    if (mapMember == null)
                        continue;

                    MRMTuple mMapperTuple = mapper.GetMappingMemberMapper(mapMember);
                    if (mMapperTuple == null)
                        continue;

                    PrimitiveMemberBinder memberBinder = new PrimitiveMemberBinder(mapMember, mMapperTuple, i);
                    memberBinders.Add(memberBinder);
                }

                return memberBinders;
            }

            static MemberInfo TryGetMapMember(List<MemberInfo> members, string readerName, TypeDescriptor typeDescriptor)
            {
                MemberInfo mapMember = null;

                foreach (MemberInfo member in members)
                {
                    string columnName = null;
                    if (typeDescriptor != null)
                    {
                        PrimitivePropertyDescriptor propertyDescriptor = typeDescriptor.FindPrimitivePropertyDescriptor(member);
                        if (propertyDescriptor != null)
                            columnName = propertyDescriptor.Column.Name;
                    }

                    if (string.IsNullOrEmpty(columnName))
                    {
                        ColumnAttribute columnAttribute = member.GetCustomAttribute<ColumnAttribute>();
                        if (columnAttribute != null)
                            columnName = columnAttribute.Name;
                    }

                    if (string.IsNullOrEmpty(columnName))
                        continue;

                    if (!string.Equals(columnName, readerName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    mapMember = member;
                    break;
                }

                if (mapMember == null)
                {
                    mapMember = members.Find(a => a.Name == readerName);
                }

                if (mapMember == null)
                {
                    mapMember = members.Find(a => string.Equals(a.Name, readerName, StringComparison.OrdinalIgnoreCase));
                }

                return mapMember;
            }

            static CacheInfo TryGetCacheInfoFromList(List<CacheInfo> caches, IDataReader reader)
            {
                CacheInfo cache = null;
                for (int i = 0; i < caches.Count; i++)
                {
                    var item = caches[i];
                    if (item.IsTheSameFields(reader))
                    {
                        cache = item;
                        break;
                    }
                }

                return cache;
            }

            static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, List<CacheInfo>> ObjectActivatorCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, List<CacheInfo>>();
        }

        public class CacheInfo
        {
            ReaderFieldInfo[] _readerFields;
            ComplexObjectActivator _objectActivator;
            public CacheInfo(ComplexObjectActivator activator, IDataReader reader)
            {
                int fieldCount = reader.FieldCount;
                var readerFields = new ReaderFieldInfo[fieldCount];

                for (int i = 0; i < fieldCount; i++)
                {
                    readerFields[i] = new ReaderFieldInfo(reader.GetName(i), reader.GetFieldType(i));
                }

                this._readerFields = readerFields;
                this._objectActivator = activator;
            }

            public ComplexObjectActivator ObjectActivator { get { return this._objectActivator; } }

            public bool IsTheSameFields(IDataReader reader)
            {
                ReaderFieldInfo[] readerFields = this._readerFields;
                int fieldCount = reader.FieldCount;

                if (fieldCount != readerFields.Length)
                    return false;

                for (int i = 0; i < fieldCount; i++)
                {
                    ReaderFieldInfo readerField = readerFields[i];
                    if (reader.GetFieldType(i) != readerField.Type || reader.GetName(i) != readerField.Name)
                    {
                        return false;
                    }
                }

                return true;
            }

            class ReaderFieldInfo
            {
                string _name;
                Type _type;
                public ReaderFieldInfo(string name, Type type)
                {
                    this._name = name;
                    this._type = type;
                }

                public string Name { get { return this._name; } }
                public Type Type { get { return this._type; } }
            }
        }
    }
}
