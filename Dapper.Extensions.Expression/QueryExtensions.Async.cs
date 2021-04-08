using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extensions.Expression
{
    public static partial class QueryExtensions
    {
        /// <summary>
        /// 当写入多条数据时，实质是循环写入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="entity"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static Task<int> InsertAsync<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string tableName = BuildInsertSql<T>(connection, out StringBuilder columnList, out StringBuilder parameterList);
            string cmd = $"insert into {tableName} ({columnList}) values ({parameterList})";
            return connection.ExecuteAsync(cmd, entity, transaction, commandTimeout);
        }

        /// <summary>
        /// 异步批量写入-一次写入多行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static async Task<int> InsertBulkAsync<T>(this IDbConnection connection, IList<T> entities, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            string tableName = GetEntityPropertyInfos<T>(connection, out StringBuilder columnList, out IList<PropertyInfo> validPropertyInfos);
            IEnumerable<Task<int>> tasks = CreateInsertTasks(connection, tableName, columnList, validPropertyInfos, entities, transaction, commandTimeout);
            int[] result = await Task.WhenAll(tasks);
            return result.Sum();
        }

        /// <summary>
        /// 创建写入任务
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
        private static IEnumerable<Task<int>> CreateInsertTasks<T>(IDbConnection connection, string tableName, StringBuilder columnList, IList<PropertyInfo> propertyInfos, IList<T> entities, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            int total = entities.Count;
            const int maxSize = 4000;
            int batchSize = Math.DivRem(total, maxSize, out int result);
            if (result != 0)
            {
                batchSize += 1;
            }
            for (int i = 0; i < batchSize; i++)
            {
                IList<T> toInsertList = entities.Skip(i * maxSize).Take(maxSize).ToList();
                yield return InternalInsertBulkAsync(connection, tableName, columnList, propertyInfos, toInsertList, transaction, commandTimeout);
            }
        }

        /// <summary>
        /// 批量写入实现
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
        private static Task<int> InternalInsertBulkAsync<T>(IDbConnection connection, string tableName, StringBuilder columnList, IList<PropertyInfo> propertyInfos, IList<T> entities, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            StringBuilder parameterList = BuildInsertBulkSql(entities, propertyInfos, out DynamicParameters parameters);
            string cmd = $"insert into {tableName} ({columnList}) values {parameterList}";
            return connection.ExecuteAsync(cmd, parameters, transaction, commandTimeout);
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
        public static Task<int> UpdateAsync<T>(this IDbConnection connection, T entityToUpdate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string sql = BuildUpdateSql(connection, entityToUpdate, out DynamicParameters parameters);
            return connection.ExecuteAsync(sql, parameters, transaction, commandTimeout);
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
        public static Task<int> UpdateAsync<T>(this IDbConnection connection, Expression<Func<T, bool>> condition, Expression<Func<T, object>> content, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string sql = BuildUpdateSql(connection, condition, content, out DynamicParameters parameters);
            return connection.ExecuteAsync(sql, parameters, transaction, commandTimeout);
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
        public static Task<int> UpdateAsync<T>(this IDbConnection connection, Expression<Func<T, bool>> condition, Expression<Func<T, T>> content, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string sql = BuildUpdateSql(connection, condition, content, out DynamicParameters parameters);
            return connection.ExecuteAsync(sql, parameters, transaction, commandTimeout);
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
        public static Task<int> DeleteAsync<T>(this IDbConnection connection, T entityToDelete, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string sql = BuildDeleteSql(connection, entityToDelete, out DynamicParameters parameters);
            return connection.ExecuteAsync(sql, parameters, transaction, commandTimeout);
        }

        /// <summary>
        /// Delete all entities in the table related to the type T.
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="transaction">The transaction to run under, null (the default) if none</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns>true if deleted, false if none found</returns>
        public static Task<int> DeleteAllAsync<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string statement = BuildDeleteAllSql<T>(connection);
            return connection.ExecuteAsync(statement, null, transaction, commandTimeout);
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
        public static Task<int> DeleteAsync<T>(this IDbConnection connection, Expression<Func<T, bool>> condition, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string statement = BuildDeleteSql(connection, condition, out DynamicParameters parameters);
            return connection.ExecuteAsync(statement, parameters, transaction, commandTimeout);
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
        public static Task<T> GetAsync<T>(this IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string sql = BuildGetQuerySql<T>(connection, id, out DynamicParameters dynParams);
            return connection.QueryFirstOrDefaultAsync<T>(sql, dynParams, transaction, commandTimeout);
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
        public static Task<IEnumerable<T>> GetAllAsync<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string sql = BuildGetAllSql<T>(connection);
            return connection.QueryAsync<T>(sql, null, transaction, commandTimeout);
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
        public static Task<int> GetCountAsync<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            string sql = BuildGetCountQuerySql<T>(connection);
            return connection.QueryFirstAsync<int>(sql, null, transaction, commandTimeout);
        }

        /// <summary>
        /// 创建query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static async Task<IList<T>> ToListAsync<T>(this IQuery query)
        {
            string commandText = query.GetCommandText();
            IEnumerable<T> result = await query.Connection.QueryAsync<T>(commandText, query.Parameters);
            return result.AsList();
        }

        /// <summary>
        /// 创建query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static Task<int> CountAsync(this IQuery query)
        {
            string commandText = query.GetCountCommandText();
            return query.Connection.QueryFirstAsync<int>(commandText, query.Parameters);
        }

        /// <summary>
        /// 创建query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static async Task<bool> AnyAsync(this IQuery query)
        {
            string commandText = query.GetCountCommandText();
            int result = await query.Connection.QueryFirstAsync<int>(commandText, query.Parameters);
            return result > 0;
        }

        /// <summary>
        /// 查询单个
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static Task<T> FirstOrDefaultAsync<T>(this IQuery query)
        {
            string commandText = query.GetCommandText();
            return query.Connection.QueryFirstOrDefaultAsync<T>(commandText, query.Parameters);
        }
    }
}
