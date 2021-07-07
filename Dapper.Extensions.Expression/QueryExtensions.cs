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
            string tableName = BuildInsertSql<T>(connection, out StringBuilder columnList, out StringBuilder parameterList);
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
        private static string BuildInsertSql<T>(IDbConnection connection, out StringBuilder columnList, out StringBuilder parameterList)
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
                parameterList.AppendFormat("@{0}", property.Name);
                if (i < canWriteProperties.Count - 1)
                {
                    parameterList.Append(", ");
                }
            }
            string name = TypeProvider.GetTableName(type);
            return adapter.GetQuoteName(name);
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
            string tableName = GetEntityPropertyInfos<T>(connection, out StringBuilder columnList, out IList<PropertyInfo> validPropertyInfos);
            int total = entities.Count;
            const int maxSize = 4000;
            int batchSize = Math.DivRem(total, maxSize, out int result);
            if (result != 0)
            {
                batchSize += 1;
            }
            int insertCount = 0;
            for (int i = 0; i < batchSize; i++)
            {
                IList<T> toInsertList = entities.Skip(i * maxSize).Take(maxSize).ToList();
                insertCount += InternalInsertBulk(connection, tableName, columnList, validPropertyInfos, toInsertList, transaction, commandTimeout);
            }
            return insertCount;
        }

        /// <summary>
        /// 获取批量写入的实体属性信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="columnList"></param>
        /// <param name="canWriteProperties"></param>
        /// <returns></returns>
        private static string GetEntityPropertyInfos<T>(IDbConnection connection, out StringBuilder columnList, out IList<PropertyInfo> canWriteProperties)
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
            string tableName = TypeProvider.GetTableName(type);
            return adapter.GetQuoteName(tableName);
        }

        /// <summary>
        /// 批量写入数据内部实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="tableName"></param>
        /// <param name="columnList"></param>
        /// <param name="propertyInfos"></param>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        private static int InternalInsertBulk<T>(IDbConnection connection, string tableName, StringBuilder columnList, IList<PropertyInfo> propertyInfos, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            StringBuilder parameterList = BuildInsertBulkSql(entities, propertyInfos, out DynamicParameters parameters);
            string cmd = $"insert into {tableName} ({columnList}) values {parameterList}";
            return connection.Execute(cmd, parameters, transaction, commandTimeout);
        }

        /// <summary>
        /// 创建写入sql和参数等
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="propertyInfos"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static StringBuilder BuildInsertBulkSql<T>(IEnumerable<T> entities, IList<PropertyInfo> propertyInfos, out DynamicParameters parameters)
        {
            StringBuilder parameterList = new StringBuilder();
            parameters = new DynamicParameters();
            int index = 0;
            foreach (T entity in entities)
            {
                if (index > 0)
                {
                    parameterList.Append(",");
                }
                parameterList.Append("(");
                for (int i = 0; i < propertyInfos.Count; i++)
                {
                    MemberInfo property = propertyInfos[i];
                    string parameterName = $"@{property.Name}_{index}";
                    parameters.Add(parameterName, property.GetValue(entity));
                    parameterList.Append(parameterName);
                    if (i < propertyInfos.Count - 1)
                    {
                        parameterList.Append(", ");
                    }
                }
                parameterList.Append(")");
                index++;
            }
            return parameterList;
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
            ISqlAdapter adapter = SqlProvider.GetFormatter(connection);
            string name = TypeProvider.GetTableName(type);
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("update {0} set ", adapter.GetQuoteName(name));
            IList<PropertyInfo> nonIdProps = TypeProvider.GetCanUpdateProperties(type);
            IList<PropertyInfo> keyProperties = TypeProvider.GetUpdateKeyProperties(type);

            parameters = new DynamicParameters();
            for (int i = 0; i < nonIdProps.Count; i++)
            {
                MemberInfo property = nonIdProps[i];
                adapter.AppendColumnNameEqualsValue(sb, property, out string columnName);
                parameters.Add("@" + columnName, property.GetValue(entity));
                if (i < nonIdProps.Count - 1)
                    sb.Append(", ");
            }
            sb.Append(" where ");
            for (int i = 0; i < keyProperties.Count; i++)
            {
                MemberInfo property = keyProperties[i];
                adapter.AppendColumnNameEqualsValue(sb, property, out string columnName);
                parameters.Add("@" + columnName, property.GetValue(entity));
                if (i < keyProperties.Count - 1)
                    sb.Append(" and ");
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

            string name = TypeProvider.GetTableName(typeof(T));
            sb.AppendFormat("update {0} set ", adapter.GetQuoteName(name));
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

            string name = TypeProvider.GetTableName(typeof(T));
            sb.AppendFormat("update {0} set ", adapter.GetQuoteName(name));
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
            IList<PropertyInfo> keyProperties = TypeProvider.GetUpdateKeyProperties(type);
            ISqlAdapter adapter = SqlProvider.GetFormatter(connection);
            string name = adapter.GetQuoteName(TypeProvider.GetTableName(type));
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("delete from {0} where ", name);
            parameters = new DynamicParameters();
            for (int i = 0; i < keyProperties.Count; i++)
            {
                MemberInfo property = keyProperties[i];
                adapter.AppendColumnNameEqualsValue(sb, property, out string columnName);
                parameters.Add("@" + columnName, property.GetValue(entity));
                if (i < keyProperties.Count - 1)
                    sb.Append(" and ");
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
            string name = TypeProvider.GetTableName(type);
            ISqlAdapter adapter = SqlProvider.GetFormatter(connection);
            string statement = $"delete from {adapter.GetQuoteName(name)}";
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
            string name = TypeProvider.GetTableName(typeof(T));
            sb.AppendFormat("delete from {0} where ", adapter.GetQuoteName(name));
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
        public static T Get<T>(this IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string sql = BuildGetQuerySql<T>(connection, id, out DynamicParameters dynParams);
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
        private static string BuildGetQuerySql<T>(IDbConnection connection, dynamic id, out DynamicParameters parameters)
        {
            Type type = typeof(T);
            if (!GetQueries.TryGetValue(type.TypeHandle, out string sql))
            {
                ISqlAdapter sqlAdapter = SqlProvider.GetFormatter(connection);
                PropertyInfo key = TypeProvider.GetSingleKey<T>(nameof(Get));
                string tableName = sqlAdapter.GetQuoteName(TypeProvider.GetTableName(type));
                string keyName = sqlAdapter.GetQuoteName(key.Name);
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
                        sqlBuilder.Append(",");
                    }
                }
                sqlBuilder.AppendFormat(" FROM {0} WHERE {1} = @id", tableName, keyName);
                sql = sqlBuilder.ToString();
                GetQueries[type.TypeHandle] = sql;
            }
            parameters = new DynamicParameters();
            parameters.Add("@id", id);
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
                    sqlBuilder.Append(",");
                }
            }
            string tableName = sqlAdapter.GetQuoteName(TypeProvider.GetTableName(type));
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
            string name = sqlAdapter.GetQuoteName(TypeProvider.GetTableName(type));
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
            string name = TypeProvider.GetTableName(typeof(T));
            ISqlAdapter adapter = SqlProvider.GetFormatter(connection);
            return adapter.GetQuoteName(name);
        }

        /// <summary>
        /// 两表联合查询
        /// </summary>
        /// <returns></returns>
        public static Query<T1, T2> JoinQuery<T1, T2>(this IDbConnection connection, JoinType joinType, Expression<Func<T1, T2, bool>> predicate)
        {
            return new Query<T1, T2>(connection, joinType, predicate);
        }

        /// <summary>
        /// 三表联合查询
        /// </summary>
        /// <returns></returns>
        public static Query<T1, T2, T3> JoinQuery<T1, T2, T3>(this IDbConnection connection, JoinType joinType, Expression<Func<T1, T2, bool>> predicate, JoinType joinType1, Expression<Func<T1, T2, T3, bool>> predicate1)
        {
            return new Query<T1, T2, T3>(connection, joinType, predicate, joinType1, predicate1);
        }
    }
}
