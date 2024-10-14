using Dapper.Extensions.Expression.Adapters;
using Dapper.Extensions.Expression.Providers;
using Dapper.Extensions.Expression.Visitors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Dapper.Extensions.Expression.Extensions;
using Dapper.Extensions.Expression.Queries;
using Dapper.Extensions.Expression.Queries.JoinQueries;

namespace Dapper.Extensions.Expression
{
    public static partial class QueryExtensions
    {
        /// <summary>
        /// get查询语句缓存
        /// </summary>
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> GetQueries = new ConcurrentDictionary<RuntimeTypeHandle, string>();

        /// <summary>
        /// count查询语句缓存
        /// </summary>
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> CountQueries = new ConcurrentDictionary<RuntimeTypeHandle, string>();

        /// <summary>
        /// 写入一条或多条记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="entity"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Insert<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string tableName = BuildInsertSql<T>(connection, entity, out StringBuilder columnList, out StringBuilder parameterList);
            string cmd = $"insert into {tableName} ({columnList}) values ({parameterList})";
            return connection.Execute(cmd, entity, transaction, commandTimeout);
        }

        /// <summary>
        /// 创建新增sql语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="columnList"></param>
        /// <param name="parameterList"></param>
        /// <returns></returns>
        private static string BuildInsertSql<T>(IDbConnection connection, T entity, out StringBuilder columnList, out StringBuilder parameterList)
        {
            Type type = typeof(T);
            if (type.IsList(out Type eleType))
            {
                type = eleType;
            }
            columnList = new StringBuilder();
            IList<PropertyInfo> canWriteProperties = TypeProvider.GetCanWriteProperties(type);
            ISqlAdapter adapter = SqlProvider.GetFormatter(connection);
            for (int i = 0; i < canWriteProperties.Count; i++)
            {
                PropertyInfo property = canWriteProperties[i];
                adapter.AppendColumnName(columnList, property);
                if (i < canWriteProperties.Count - 1)
                {
                    columnList.Append(", ");
                }
            }
            parameterList = new StringBuilder();
            for (int i = 0; i < canWriteProperties.Count; i++)
            {
                PropertyInfo property = canWriteProperties[i];
                object value = property.GetValue(entity);
                if (value == null)
                {
                    parameterList.Append("NULL");
                }
                else if (value is bool v)
                {
                    parameterList.Append(adapter.ParseBool(v));
                }
                else if (value is Guid v1)
                {
                    parameterList.Append('\'').Append(v1).Append('\'');
                }
                else if (value.GetType().IsEnum)
                {
                    parameterList.Append(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())));
                }
                else if (value.GetType().IsNumericType())
                {
                    parameterList.Append(value);
                }
                else
                {
                    adapter.AddParameter(parameterList, property.Name);
                }
                if (i < canWriteProperties.Count - 1)
                {
                    parameterList.Append(", ");
                }
            }
            return adapter.GetTableName(type);
        }

        /// <summary>
        /// 批量写入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int InsertBulk<T>(this IDbConnection connection, IList<T> entities, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string tableName = GetEntityPropertyInfos<T>(connection, out StringBuilder columnList, out IList<PropertyInfo> validPropertyInfos, out int maxParameterCount);
            StringBuilder parameterList = new StringBuilder();
            ISqlAdapter adapter = SqlProvider.GetFormatter(connection);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            int index = 0;
            int count = 0;
            foreach (T entity in entities)
            {
                if (parameterList.Length > 0)
                {
                    parameterList.Append(',');
                }
                parameterList.Append('(');
                for (int i = 0; i < validPropertyInfos.Count; i++)
                {
                    if (i > 0 && i < validPropertyInfos.Count)
                    {
                        parameterList.Append(", ");
                    }
                    MemberInfo property = validPropertyInfos[i];
                    object value = property.GetValue(entity);

                    if (value == null)
                    {
                        parameterList.Append("NULL");
                        continue;
                    }
                    if (value is bool v)
                    {
                        parameterList.Append(adapter.ParseBool(v));
                        continue;
                    }
                    if (value is Guid v1)
                    {
                        parameterList.Append('\'').Append(v1).Append('\'');
                        continue;
                    }
                    Type valType = value.GetType();
                    if (valType.IsEnum)
                    {
                        value = Convert.ChangeType(value, Enum.GetUnderlyingType(valType));
                        valType = valType.GetType();
                    }
                    if (value.GetType().IsNumericType())
                    {
                        parameterList.Append(value);
                        continue;
                    }
                    string parameterName = $"{property.Name}_{index}";
                    adapter.AddParameter(parameterList, parameterName);
                    adapter.AddParameter(parameters, parameterName, value);
                }
                parameterList.Append(')');
                if (parameters.Count > maxParameterCount || index + 1 == entities.Count)
                {
                    string cmd = $"insert into {tableName} ({columnList}) values {parameterList}";
                    count += connection.Execute(cmd, parameters, transaction, commandTimeout);
                    parameterList.Clear();
                    parameters.Clear();
                }
                index++;
            }
            return count;
        }

        /// <summary>
        /// 获取批量写入的实体属性信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="columnList"></param>
        /// <param name="canWriteProperties"></param>
        /// <returns></returns>
        private static string GetEntityPropertyInfos<T>(IDbConnection connection, out StringBuilder columnList, out IList<PropertyInfo> canWriteProperties, out int maxParameterCount)
        {
            Type type = typeof(T);
            if (type.IsList(out Type eleType))
            {
                type = eleType;
            }
            columnList = new StringBuilder();
            canWriteProperties = TypeProvider.GetCanWriteProperties(type);
            ISqlAdapter adapter = SqlProvider.GetFormatter(connection);
            for (int i = 0; i < canWriteProperties.Count; i++)
            {
                PropertyInfo property = canWriteProperties[i];
                adapter.AppendColumnName(columnList, property);
                if (i < canWriteProperties.Count - 1)
                {
                    columnList.Append(", ");
                }
            }
            maxParameterCount = adapter.MaxParameterCount;
            return adapter.GetTableName(type);
        }

        /// <summary>
        /// Updates entity in table "Ts", checks if the entity is modified if the entity is tracked by the Get() extension.
        /// </summary>
        /// <typeparam name="T">Type to be updated</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToUpdate">Entity to be updated</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>true if updated, false if not found or not modified (tracked entities)</returns>
        public static int Update<T>(this IDbConnection connection, T entityToUpdate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string sql = BuildUpdateSql(connection, entityToUpdate, out DynamicParameters parameters);
            return connection.Execute(sql, parameters, transaction, commandTimeout);
        }

        /// <summary>
        /// 创建更新sql语句
        /// </summary>
        /// <returns></returns>
        private static string BuildUpdateSql<T>(IDbConnection connection, T entity, out DynamicParameters parameters)
        {
            Type type = typeof(T);
            if (type.IsList(out Type eleType))
            {
                type = eleType;
            }
            IList<PropertyInfo> keyProperties = TypeProvider.GetKeyProperties(type);
            if (!keyProperties.Any())
            {
                throw new DataException($"{type} only supports an entity with a [Key] property");
            }
            IList<PropertyInfo> canUpdateProperties = TypeProvider.GetCanUpdateProperties(type);
            if (!canUpdateProperties.Any())
            {
                throw new DataException($"{type} no columns to update");
            }
            ISqlAdapter adapter = SqlProvider.GetFormatter(connection);
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("update {0} set ", adapter.GetTableName(type));

            parameters = new DynamicParameters();
            for (int i = 0; i < canUpdateProperties.Count; i++)
            {
                MemberInfo property = canUpdateProperties[i];
                adapter.AppendBinaryColumn(sb, property, out string columnName);
                adapter.AddParameter(parameters, columnName, property.GetValue(entity));
                if (i < canUpdateProperties.Count - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(" where ");
            for (int i = 0; i < keyProperties.Count; i++)
            {
                MemberInfo property = keyProperties[i];
                adapter.AppendBinaryColumn(sb, property, out string columnName);
                adapter.AddParameter(parameters, columnName, property.GetValue(entity));
                if (i < keyProperties.Count - 1)
                {
                    sb.Append(" and ");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Updates entity in table "Ts", checks if the entity is modified if the entity is tracked by the Get() extension.
        /// </summary>
        /// <typeparam name="T">Type to be updated</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="condition">更新条件</param>
        /// <param name="content">更新内容</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>true if updated, false if not found or not modified (tracked entities)</returns>
        public static int Update<T>(this IDbConnection connection, Expression<Func<T, bool>> condition, Expression<Func<T, object>> content, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string sql = BuildUpdateSql(connection, condition, content, out DynamicParameters parameters);
            return connection.Execute(sql, parameters, transaction, commandTimeout);
        }

        /// <summary>
        /// 创建表达式查询sql语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="content"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static string BuildUpdateSql<T>(IDbConnection connection, Expression<Func<T, bool>> condition, Expression<Func<T, object>> content, out DynamicParameters parameters)
        {
            ISqlAdapter adapter = SqlProvider.GetFormatter(connection);

            StringBuilder sb = new StringBuilder();
            parameters = new DynamicParameters();

            string tableName = adapter.GetTableName(typeof(T));
            sb.AppendFormat("update {0} set ", tableName);
            UpdateExpressionVisitor.Visit(content, adapter, sb, parameters);

            sb.AppendFormat(" where ");
            WhereExpressionVisitor.Visit(condition, adapter, sb, parameters, false);

            return sb.ToString();
        }

        /// <summary>
        /// Updates entity in table "Ts", checks if the entity is modified if the entity is tracked by the Get() extension.
        /// </summary>
        /// <typeparam name="T">Type to be updated</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="condition">更新条件</param>
        /// <param name="content">更新内容</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>true if updated, false if not found or not modified (tracked entities)</returns>
        public static int Update<T>(this IDbConnection connection, Expression<Func<T, bool>> condition, Expression<Func<T, T>> content, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string sql = BuildUpdateSql(connection, condition, content, out DynamicParameters parameters);
            return connection.Execute(sql, parameters, transaction, commandTimeout);
        }

        /// <summary>
        /// 创建更新sql语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="content"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static string BuildUpdateSql<T>(IDbConnection connection, Expression<Func<T, bool>> condition, Expression<Func<T, T>> content, out DynamicParameters parameters)
        {
            ISqlAdapter adapter = SqlProvider.GetFormatter(connection);
            StringBuilder sb = new StringBuilder();
            parameters = new DynamicParameters();

            string tableName = adapter.GetTableName(typeof(T));
            sb.AppendFormat("update {0} set ", tableName);
            UpdateExpressionVisitor.Visit(content, adapter, sb, parameters);

            sb.Append(" where ");
            WhereExpressionVisitor.Visit(condition, adapter, sb, parameters, false);

            return sb.ToString();
        }

        /// <summary>
        /// Delete entity in table "Ts".
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToDelete">Entity to delete</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>true if deleted, false if not found</returns>
        public static int Delete<T>(this IDbConnection connection, T entityToDelete, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string sql = BuildDeleteSql(connection, entityToDelete, out DynamicParameters parameters);
            return connection.Execute(sql, parameters, transaction, commandTimeout);
        }

        /// <summary>
        /// 创建删除sql
        /// </summary>
        /// <returns></returns>
        private static string BuildDeleteSql<T>(IDbConnection connection, T entity, out DynamicParameters parameters)
        {
            Type type = typeof(T);

            if (type.IsList(out Type eleType))
            {
                type = eleType;
            }
            IList<PropertyInfo> keyProperties = TypeProvider.GetKeyProperties(type);
            if (!keyProperties.Any())
            {
                throw new DataException($"{type} only supports an entity with a [Key] property");
            }
            ISqlAdapter adapter = SqlProvider.GetFormatter(connection);
            string tableName = adapter.GetTableName(type);
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("delete from {0} where ", tableName);
            parameters = new DynamicParameters();
            for (int i = 0; i < keyProperties.Count; i++)
            {
                MemberInfo property = keyProperties[i];
                adapter.AppendBinaryColumn(sb, property, out string columnName);
                adapter.AddParameter(parameters, columnName, property.GetValue(entity));
                if (i < keyProperties.Count - 1)
                {
                    sb.Append(" and ");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Delete all entities in the table related to the type T.
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>true if deleted, false if none found</returns>
        public static int DeleteAll<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string statement = BuildDeleteAllSql<T>(connection);
            return connection.Execute(statement, null, transaction, commandTimeout);
        }

        /// <summary>
        /// 创建删除全部的sql语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <returns></returns>
        private static string BuildDeleteAllSql<T>(IDbConnection connection)
        {
            Type type = typeof(T);
            ISqlAdapter adapter = SqlProvider.GetFormatter(connection);
            string statement = $"delete from {adapter.GetTableName(type)}";
            return statement;
        }

        /// <summary>
        /// Updates entity in table "Ts", checks if the entity is modified if the entity is tracked by the Get() extension.
        /// </summary>
        /// <typeparam name="T">Type to be updated</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="condition">删除</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>true if updated, false if not found or not modified (tracked entities)</returns>
        public static int Delete<T>(this IDbConnection connection, Expression<Func<T, bool>> condition, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string statement = BuildDeleteSql(connection, condition, out DynamicParameters parameters);
            int updated = connection.Execute(statement, parameters, transaction, commandTimeout);
            return updated;
        }

        /// <summary>
        /// 创建表达式删除sql语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static string BuildDeleteSql<T>(IDbConnection connection, Expression<Func<T, bool>> condition, out DynamicParameters parameters)
        {
            ISqlAdapter adapter = SqlProvider.GetFormatter(connection);
            StringBuilder sb = new StringBuilder();
            parameters = new DynamicParameters();
            sb.AppendFormat("delete from {0} where ", adapter.GetTableName(typeof(T)));
            WhereExpressionVisitor.Visit(condition, adapter, sb, parameters, false);
            return sb.ToString();
        }

        /// <summary>
        /// Returns a single entity by a single id from table "Ts".  
        /// Id must be marked with [Key] attribute.
        /// Entities created from interfaces are tracked/intercepted for changes and used by the Update() extension
        /// for optimal performance. 
        /// </summary>
        /// <typeparam name="T">Interface or type to create and populate</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="id">Id of the entity to get, must be marked with [Key] attribute</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>Entity of T</returns>
        public static T Get<T>(this IDbConnection connection, Expression<Func<T, bool>> condition, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string sql = BuildGetQuerySql<T>(connection, condition, out DynamicParameters dynParams);
            return connection.QueryFirstOrDefault<T>(sql, dynParams, transaction, commandTimeout);
        }

        /// <summary>
        /// 获取按Id查询sql语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static string BuildGetQuerySql<T>(IDbConnection connection, Expression<Func<T, bool>> condition, out DynamicParameters parameters)
        {
            parameters = new DynamicParameters();
            Type type = typeof(T);
            if (!GetQueries.TryGetValue(type.TypeHandle, out string sql))
            {
                ISqlAdapter sqlAdapter = SqlProvider.GetFormatter(connection);
                IList<PropertyInfo> queryProperties = TypeProvider.GetCanQueryProperties(type);
                StringBuilder sqlBuilder = new StringBuilder();
                sqlBuilder.Append("SELECT ");
                foreach (PropertyInfo propertyInfo in queryProperties)
                {
                    bool isAlias = sqlAdapter.AppendColumnName(sqlBuilder, propertyInfo);
                    if (isAlias)
                    {
                        sqlBuilder.Append(" AS ");
                        sqlAdapter.AppendQuoteName(sqlBuilder, propertyInfo.Name);
                    }
                    if (queryProperties.IndexOf(propertyInfo) < queryProperties.Count - 1)
                    {
                        sqlBuilder.Append(',');
                    }
                }
                string tableName = sqlAdapter.GetTableName(type);
                sqlBuilder.AppendFormat(" FROM {0} WHERE ", tableName);
                WhereExpressionVisitor.Visit(condition, sqlAdapter, sqlBuilder, parameters, false);
                sql = sqlBuilder.ToString();
                GetQueries[type.TypeHandle] = sql;
            }
            return sql;
        }

        /// <summary>
        /// Returns a list of entities from table "Ts".
        /// Id of T must be marked with [Key] attribute.
        /// Entities created from interfaces are tracked/intercepted for changes and used by the Update() extension
        /// for optimal performance.
        /// </summary>
        /// <typeparam name="T">Interface or type to create and populate</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>Entity of T</returns>
        public static IEnumerable<T> GetAll<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string sql = BuildGetAllSql<T>(connection);
            return connection.Query<T>(sql, null, transaction, commandTimeout: commandTimeout);
        }

        /// <summary>
        /// 创建获取所有的sql语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <returns></returns>
        private static string BuildGetAllSql<T>(IDbConnection connection)
        {
            Type type = typeof(T);
            Type cacheType = typeof(List<T>);
            if (GetQueries.TryGetValue(cacheType.TypeHandle, out string sql))
            {
                return sql;
            }
            ISqlAdapter sqlAdapter = SqlProvider.GetFormatter(connection);
            IList<PropertyInfo> queryProperties = TypeProvider.GetCanQueryProperties(type);
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append("SELECT ");
            foreach (PropertyInfo propertyInfo in queryProperties)
            {
                bool isAlias = sqlAdapter.AppendColumnName(sqlBuilder, propertyInfo);
                if (isAlias)
                {
                    sqlBuilder.Append(" AS ");
                    sqlAdapter.AppendQuoteName(sqlBuilder, propertyInfo.Name);
                }
                if (queryProperties.IndexOf(propertyInfo) < queryProperties.Count - 1)
                {
                    sqlBuilder.Append(',');
                }
            }
            string tableName = sqlAdapter.GetTableName(type);
            sqlBuilder.AppendFormat(" FROM {0}", tableName);
            sql = sqlBuilder.ToString();
            GetQueries[cacheType.TypeHandle] = sql;
            return sql;
        }

        /// <summary>
        /// Returns a single entity by a single id from table "Ts".  
        /// Id must be marked with [Key] attribute.
        /// Entities created from interfaces are tracked/intercepted for changes and used by the Update() extension
        /// for optimal performance. 
        /// </summary>
        /// <typeparam name="T">Interface or type to create and populate</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>Entity of T</returns>
        public static int GetCount<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string sql = BuildGetCountQuerySql<T>(connection);
            return connection.QueryFirst<int>(sql, null, transaction, commandTimeout);
        }

        /// <summary>
        /// 获取按Id查询sql语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <returns></returns>
        private static string BuildGetCountQuerySql<T>(IDbConnection connection)
        {
            Type type = typeof(T);
            if (CountQueries.TryGetValue(type.TypeHandle, out string sql))
            {
                return sql;
            }
            ISqlAdapter sqlAdapter = SqlProvider.GetFormatter(connection);
            string name = sqlAdapter.GetTableName(type);
            sql = $"select count(*) from {name}";
            CountQueries[type.TypeHandle] = sql;
            return sql;
        }

        /// <summary>
        /// 创建query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static Query<T> Query<T>(this IDbConnection connection)
        {
            return new Query<T>(connection);
        }

        /// <summary>
        /// 创建query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IList<T> ToList<T>(this IQuery query)
        {
            string commandText = query.GetCommandText();
            Debug.WriteLine(commandText);
            IEnumerable<T> result = query.Connection.Query<T>(commandText, query.Parameters);
            return result.AsList();
        }

        /// <summary>
        /// 创建query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static int Count(this IQuery query)
        {
            string commandText = query.GetCountCommandText();
            Debug.WriteLine(commandText);
            return query.Connection.QueryFirst<int>(commandText, query.Parameters);
        }

        /// <summary>
        /// 创建query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static bool Any(this IQuery query)
        {
            string commandText = query.GetCountCommandText();
            Debug.WriteLine(commandText);
            int result = query.Connection.QueryFirst<int>(commandText, query.Parameters);
            return result > 0;
        }

        /// <summary>
        /// 查询单个
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static T FirstOrDefault<T>(this IQuery query)
        {
            string commandText = query.GetCommandText();
            Debug.WriteLine(commandText);
            return query.Connection.QueryFirstOrDefault<T>(commandText, query.Parameters);
        }

        /// <summary>
        /// 获取数据库表名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static string GetTableName<T>(this IDbConnection connection)
        {
            ISqlAdapter adapter = SqlProvider.GetFormatter(connection);
            return adapter.GetTableName(typeof(T));
        }

        /// <summary>
        /// 2表联合查询
        /// </summary>
        /// <returns></returns>
        public static JoinQuery<T1, T2> JoinQuery<T1, T2>(this IDbConnection connection)
        {
            return new JoinQuery<T1, T2>(connection);
        }

        /// <summary>
        /// 3表联合查询
        /// </summary>
        /// <returns></returns>
        public static JoinQuery<T1, T2, T3> JoinQuery<T1, T2, T3>(this IDbConnection connection)
        {
            return new JoinQuery<T1, T2, T3>(connection);
        }

        /// <summary>
        /// 4表联合查询
        /// </summary>
        /// <returns></returns>
        public static JoinQuery<T1, T2, T3, T4> JoinQuery<T1, T2, T3, T4>(this IDbConnection connection)
        {
            return new JoinQuery<T1, T2, T3, T4>(connection);
        }

        /// <summary>
        /// 5表联合查询
        /// </summary>
        /// <returns></returns>
        public static JoinQuery<T1, T2, T3, T4, T5> JoinQuery<T1, T2, T3, T4, T5>(this IDbConnection connection)
        {
            return new JoinQuery<T1, T2, T3, T4, T5>(connection);
        }

        /// <summary>
        /// 6表联合查询
        /// </summary>
        /// <returns></returns>
        public static JoinQuery<T1, T2, T3, T4, T5, T6> JoinQuery<T1, T2, T3, T4, T5, T6>(this IDbConnection connection)
        {
            return new JoinQuery<T1, T2, T3, T4, T5, T6>(connection);
        }
    }
}
