using Dapper.Extensions.Expression.Adapters;
using Dapper.Extensions.Expression.Providers;
using Dapper.Extensions.Expression.Utilities;
using Dapper.Extensions.Expression.Visitors;
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
    public interface IQuery
    {
        /// <summary>
        /// 参数
        /// </summary>
        DynamicParameters Parameters { get; }

        /// <summary>
        /// 数据库连接
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// 获取SQL文本
        /// </summary>
        /// <returns></returns>
        string GetCommandText();

        /// <summary>
        /// 获取COUNT SQL文本
        /// </summary>
        /// <returns></returns>
        string GetCountCommandText();
    }

    public sealed class Query<T> : IQuery
    {
        /// <summary>
        /// sql适配器
        /// </summary>
        private readonly ISqlAdapter _adapter;

        /// <summary>
        /// 参数
        /// </summary>
        public DynamicParameters Parameters { get; }

        /// <summary>
        /// 数据库连接
        /// </summary>
        public IDbConnection Connection { get; }

        /// <summary>
        /// 当前页数
        /// </summary>
        private int _page;

        /// <summary>
        /// 没页条数
        /// </summary>
        private int _pageSize;

        /// <summary>
        /// 是否唯一列
        /// </summary>
        private bool _distinct;

        /// <summary>
        /// sql语句容器
        /// </summary>
        private readonly StringBuilder _selectBuilder;

        /// <summary>
        /// 排序
        /// </summary>
        private StringBuilder _orderBuilder;

        /// <summary>
        /// 分组
        /// </summary>
        private StringBuilder _groupBuilder;

        /// <summary>
        /// Having
        /// </summary>
        private StringBuilder _having;

        /// <summary>
        /// where语句
        /// </summary>
        private StringBuilder _whereBuilder;

        /// <summary>
        /// 聚合函数
        /// </summary>
        private StringBuilder _aggregateBuilder;

        internal Query(IDbConnection connection)
        {
            _adapter = SqlProvider.GetFormatter(connection);
            _selectBuilder = new StringBuilder();
            Parameters = new DynamicParameters();
            Connection = connection;
        }

        /// <summary>
        /// 设置条件
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public Query<T> Where(Expression<Func<T, bool>> ex)
        {
            if (_whereBuilder == null)
            {
                _whereBuilder = new StringBuilder();
            }
            if (_whereBuilder.Length > 0)
            {
                _whereBuilder.Append(" AND ");
            }
            WhereExpressionVisitor.Visit(ReplaceParameterVisitor.Replace(ExpressionResolver.Visit(ex), ex.Parameters), _adapter, _whereBuilder, Parameters, true);
            return this;
        }

        /// <summary>
        /// 带条件筛选
        /// </summary>
        /// <param name="condition">是否进入筛选</param>
        /// <param name="ex">表达式</param>
        /// <returns></returns>
        public Query<T> WhereIf(bool condition, Expression<Func<T, bool>> ex)
        {
            return condition ? Where(ex) : this;
        }

        /// <summary>
        /// 设置分页信息
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public Query<T> TakePage(int page, int pageSize)
        {
            if (page <= 0 || pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(page)}或{nameof(pageSize)}必须大于0");
            }
            _page = page;
            _pageSize = pageSize;
            return this;
        }

        /// <summary>
        /// 设置查询限制信息
        /// </summary>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public Query<T> Take(int pageSize)
        {
            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize) + "必须大于0");
            }
            _pageSize = pageSize;
            return this;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="column"></param>
        /// <param name="sort">排序方式 asc|desc</param>
        /// <returns></returns>
        public Query<T> OrderBy(string column, string sort)
        {
            if (_orderBuilder == null)
            {
                _orderBuilder = new StringBuilder(null);
            }
            if (string.IsNullOrWhiteSpace(column))
            {
                return this;
            }
            if (!(string.Equals("asc", sort, StringComparison.OrdinalIgnoreCase) || string.Equals("desc", sort, StringComparison.OrdinalIgnoreCase)))
            {
                throw new NotSupportedException("不支持的排序方式:" + sort);
            }
            if (_orderBuilder.Length > 0)
            {
                _orderBuilder.Append(",");
            }
            _orderBuilder.AppendFormat(" {0} {1} ", _adapter.GetQuoteName(column), sort);
            return this;
        }

        public Query<T> OrderBy<TK>(Expression<Func<T, TK>> keySelector)
        {
            if (_orderBuilder == null)
            {
                _orderBuilder = new StringBuilder(null);
            }
            if (_orderBuilder.Length > 0)
            {
                _orderBuilder.Append(",");
            }
            OrderExpressionVisitor.Visit(ReplaceParameterVisitor.Replace(keySelector, keySelector.Parameters), _adapter, null, _orderBuilder, false);
            return this;
        }

        public Query<T> OrderByDescending<TK>(Expression<Func<T, TK>> keySelector)
        {
            if (_orderBuilder == null)
            {
                _orderBuilder = new StringBuilder();
            }
            if (_orderBuilder.Length > 0)
            {
                _orderBuilder.Append(",");
            }
            OrderExpressionVisitor.Visit(ReplaceParameterVisitor.Replace(keySelector, keySelector.Parameters), _adapter, "DESC", _orderBuilder, false);
            return this;
        }

        /// <summary>
        /// 分组
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public Query<T> GroupBy(string selector)
        {
            if (string.IsNullOrWhiteSpace(selector))
            {
                return this;
            }
            if (_groupBuilder == null)
            {
                _groupBuilder = new StringBuilder();
            }
            if (_groupBuilder.Length > 0)
            {
                _groupBuilder.Clear();
            }
            _groupBuilder.AppendFormat(" {0} ", selector);
            return this;
        }

        public Query<T> GroupBy<TK>(Expression<Func<T, TK>> keySelector)
        {
            if (_groupBuilder == null)
            {
                _groupBuilder = new StringBuilder();
            }
            if (_groupBuilder.Length > 0)
            {
                _groupBuilder.Append(",");
            }
            OrderExpressionVisitor.Visit(ReplaceParameterVisitor.Replace(keySelector, keySelector.Parameters), _adapter, null, _groupBuilder, false);
            return this;
        }

        public Query<T> Having(Expression<Func<T, bool>> keySelector)
        {
            if (_having == null)
            {
                _having = new StringBuilder();
            }
            WhereExpressionVisitor.Visit(ReplaceParameterVisitor.Replace(ExpressionResolver.Visit(keySelector), keySelector.Parameters), _adapter, _having, Parameters, true);
            return this;
        }

        public TResult Max<TResult>(Expression<Func<T, TResult>> keySelector)
        {
            BuildMaxSql(keySelector);
            string commandText = GetFunctionCommandText();
            return Connection.QueryFirstOrDefault<TResult>(commandText, Parameters);
        }

        public Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> keySelector)
        {
            BuildMaxSql(keySelector);
            string commandText = GetFunctionCommandText();
            return Connection.QueryFirstOrDefaultAsync<TResult>(commandText, Parameters);
        }

        private void BuildMaxSql<TResult>(Expression<Func<T, TResult>> keySelector)
        {
            if (_aggregateBuilder == null)
            {
                _aggregateBuilder = new StringBuilder();
            }
            if (_aggregateBuilder.Length > 0)
            {
                _aggregateBuilder.Clear();
            }
            _aggregateBuilder.Append("MAX(");
            OrderExpressionVisitor.Visit(ReplaceParameterVisitor.Replace(keySelector, keySelector.Parameters), _adapter, null, _aggregateBuilder, false);
            _aggregateBuilder.Append(")");
        }

        public TResult Min<TResult>(Expression<Func<T, TResult>> keySelector)
        {
            BuildMinSql(keySelector);
            string commandText = GetFunctionCommandText();
            return Connection.QueryFirstOrDefault<TResult>(commandText, Parameters);
        }

        public Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> keySelector)
        {
            BuildMinSql(keySelector);
            string commandText = GetFunctionCommandText();
            return Connection.QueryFirstOrDefaultAsync<TResult>(commandText, Parameters);
        }

        private void BuildMinSql<TResult>(Expression<Func<T, TResult>> keySelector)
        {
            if (_aggregateBuilder == null)
            {
                _aggregateBuilder = new StringBuilder();
            }
            if (_aggregateBuilder.Length > 0)
            {
                _aggregateBuilder.Clear();
            }
            _aggregateBuilder.Append("MIN(");
            OrderExpressionVisitor.Visit(ReplaceParameterVisitor.Replace(keySelector, keySelector.Parameters), _adapter, null, _aggregateBuilder, false);
            _aggregateBuilder.Append(")");
        }

        public TResult Sum<TResult>(Expression<Func<T, TResult>> keySelector)
        {
            BuildSumSql(keySelector);
            string commandText = GetFunctionCommandText();
            return Connection.QueryFirstOrDefault<TResult>(commandText, Parameters);
        }

        public Task<TResult> SumAsync<TResult>(Expression<Func<T, TResult>> keySelector)
        {
            BuildSumSql(keySelector);
            string commandText = GetFunctionCommandText();
            return Connection.QueryFirstOrDefaultAsync<TResult>(commandText, Parameters);
        }

        private void BuildSumSql<TResult>(Expression<Func<T, TResult>> keySelector)
        {
            if (!TypeProvider.AllowTypes.Contains(typeof(TResult)))
            {
                throw new NotSupportedException("不支持的结果类型" + typeof(TResult).Name);
            }
            if (_aggregateBuilder == null)
            {
                _aggregateBuilder = new StringBuilder();
            }
            if (_aggregateBuilder.Length > 0)
            {
                _aggregateBuilder.Clear();
            }
            _aggregateBuilder.Append("SUM(");
            OrderExpressionVisitor.Visit(ReplaceParameterVisitor.Replace(keySelector, keySelector.Parameters), _adapter, null, _aggregateBuilder, true);
            _aggregateBuilder.Append(")");
        }

        public Query<T> Select(Expression<Func<T, object>> selector)
        {
            SelectExpressionVisitor.Visit(ReplaceParameterVisitor.Replace(selector, selector.Parameters), _adapter, _selectBuilder, true);
            return this;
        }

        public Query<T> Select(Expression<Func<T, T>> selector)
        {
            SelectExpressionVisitor.Visit(ReplaceParameterVisitor.Replace(selector, selector.Parameters), _adapter, _selectBuilder, true);
            return this;
        }

        public Query<T> Distinct()
        {
            _distinct = true;
            return this;
        }

        public Query<T> Exist<T1>(Expression<Func<T, T1, bool>> where)
        {
            if (_whereBuilder == null)
            {
                _whereBuilder = new StringBuilder();
            }
            if (_whereBuilder.Length > 0)
            {
                _whereBuilder.Append(" AND ");
            }
            LambdaExpression ex = ReplaceParameterVisitor.Replace(ExpressionResolver.Visit(where), where.Parameters);
            ParameterExpression p = ex.Parameters.Last();
            string tableName = TypeProvider.GetTableName(p.Type);
            _whereBuilder.AppendFormat("EXISTS (SELECT 1 FROM {0} AS {1} WHERE ", _adapter.GetQuoteName(tableName), _adapter.GetQuoteName(p.Name));
            WhereExpressionVisitor.Visit(ex, _adapter, _whereBuilder, Parameters, true);
            _whereBuilder.Append(") ");
            return this;
        }

        public Query<T> Between<TK>(Expression<Func<T, TK>> selector, TK left, TK right)
        {
            if (_whereBuilder == null)
            {
                _whereBuilder = new StringBuilder();
            }
            if (_whereBuilder.Length > 0)
            {
                _whereBuilder.Append(" AND ");
            }
            LambdaExpression ex = ReplaceParameterVisitor.Replace(selector, selector.Parameters);
            SelectExpressionVisitor.Visit(ex, _adapter, _whereBuilder, true);
            _whereBuilder.Append(" BETWEEN ");
            WhereExpressionVisitor.AddParameter(_whereBuilder, Parameters, left);
            _whereBuilder.Append(" AND ");
            WhereExpressionVisitor.AddParameter(_whereBuilder, Parameters, right);
            return this;
        }

        private void ParseSelect()
        {
            if (_selectBuilder.Length > 0)
            {
                return;
            }
            Type type = typeof(T);
            IList<PropertyInfo> validPropertyInfos = TypeProvider.GetCanQueryProperties(type);
            foreach (var property in validPropertyInfos)
            {
                _selectBuilder.Append(_adapter.GetQuoteName("t1")).Append(".");
                bool isAlias = _adapter.AppendColumnName(_selectBuilder, property);
                if (isAlias)
                {
                    _selectBuilder.Append(" AS ");
                    _adapter.AppendQuoteName(_selectBuilder, property.Name);
                }
                if (validPropertyInfos.IndexOf(property) < validPropertyInfos.Count - 1)
                {
                    _selectBuilder.Append(",");
                }
            }
        }

        public string GetCommandText()
        {
            StringBuilder sqlBuilder = new StringBuilder();
            BuildSelect(sqlBuilder);
            BuildTable(sqlBuilder);
            BuildWhere(sqlBuilder);
            BuildGroup(sqlBuilder);
            BuildOrder(sqlBuilder);
            _adapter.AppendPage(sqlBuilder, _page, _pageSize);
            return sqlBuilder.ToString();
        }

        private void BuildSelect(StringBuilder sqlBuilder)
        {
            sqlBuilder.Append("SELECT ");
            if (_distinct)
            {
                sqlBuilder.Append(" DISTINCT ");
            }
            ParseSelect();
            sqlBuilder.Append(_selectBuilder);
        }

        private void BuildTable(StringBuilder sqlBuilder)
        {
            sqlBuilder.Append(" FROM ");
            _adapter.AppendQuoteName(sqlBuilder, TypeProvider.GetTableName(typeof(T)));
            sqlBuilder.Append(" AS ").Append(_adapter.GetQuoteName("t1"));
        }

        public string GetCountCommandText()
        {
            StringBuilder countBuilder = new StringBuilder();
            countBuilder.Append("SELECT COUNT(*) ");
            BuildTable(countBuilder);
            BuildWhere(countBuilder);
            BuildGroup(countBuilder);
            return countBuilder.ToString();
        }

        private string GetFunctionCommandText()
        {
            StringBuilder functionBuilder = new StringBuilder();
            functionBuilder.Append("SELECT ").Append(_aggregateBuilder);
            BuildTable(functionBuilder);
            BuildWhere(functionBuilder);
            BuildGroup(functionBuilder);
            return functionBuilder.ToString();
        }

        private void BuildWhere(StringBuilder builder)
        {
            if (_whereBuilder != null && _whereBuilder.Length > 0)
            {
                builder.AppendFormat(" WHERE {0}", _whereBuilder);
            }
        }

        private void BuildGroup(StringBuilder builder)
        {
            if (_groupBuilder != null && _groupBuilder.Length > 0)
            {
                builder.AppendFormat(" GROUP BY {0} ", _groupBuilder);
            }
            if (_having != null && _having.Length > 0)
            {
                builder.AppendFormat(" HAVING {0} ", _having);
            }
        }

        private void BuildOrder(StringBuilder builder)
        {
            if (_orderBuilder != null && _orderBuilder.Length > 0)
            {
                builder.AppendFormat(" ORDER BY {0} ", _orderBuilder);
            }
        }
    }
}
