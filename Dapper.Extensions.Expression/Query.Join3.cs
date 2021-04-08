using Dapper.Extensions.Expression.Adapters;
using Dapper.Extensions.Expression.Providers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper.Extensions.Expression.Utilities;
using Dapper.Extensions.Expression.Visitors;

namespace Dapper.Extensions.Expression
{
    /// <summary>
    /// 三表联合查询
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    public sealed class Query<T1, T2, T3> : IQuery
    {
        /// <summary>
        /// 默认select表达式
        /// </summary>
        private static readonly Expression<Func<T1, T2, T3, T1>> DefaultSelector = (t1, t2, t3) => t1;

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

        /// <summary>
        /// 连接条件
        /// </summary>
        private readonly LambdaExpression[] _onExpressions;

        /// <summary>
        /// join类型
        /// </summary>
        private readonly JoinType[] _joinTypes;

        internal Query(IDbConnection connection, JoinType joinType, Expression<Func<T1, T2, bool>> on1, JoinType joinType1, Expression<Func<T1, T2, T3, bool>> on2)
        {
            _joinTypes = new[] { joinType, joinType1 };
            _onExpressions = new[] { ReplaceParameterVisitor.Replace(on1, on1.Parameters), ReplaceParameterVisitor.Replace(on2, on2.Parameters) };
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
        public Query<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> ex)
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

        public Query<T1, T2, T3> Exist<T4>(Expression<Func<T1, T2, T3, T4, bool>> where)
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

        public Query<T1, T2, T3> Between<TK>(Expression<Func<T1, T2, T3, TK>> selector, TK left, TK right)
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

        /// <summary>
        /// 设置分页信息
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public Query<T1, T2, T3> TakePage(int page, int pageSize)
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
        public Query<T1, T2, T3> Take(int pageSize)
        {
            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize) + "必须大于0");
            }
            _pageSize = pageSize;
            return this;
        }

        public Query<T1, T2, T3> OrderBy<TK>(Expression<Func<T1, T2, T3, TK>> keySelector)
        {
            if (_orderBuilder == null)
            {
                _orderBuilder = new StringBuilder(null);
            }
            if (_orderBuilder.Length > 0)
            {
                _orderBuilder.Append(",");
            }
            OrderExpressionVisitor.Visit(ReplaceParameterVisitor.Replace(keySelector, keySelector.Parameters), _adapter, null, _orderBuilder, true);
            return this;
        }

        public Query<T1, T2, T3> OrderByDescending<TK>(Expression<Func<T1, T2, T3, TK>> keySelector)
        {
            if (_orderBuilder == null)
            {
                _orderBuilder = new StringBuilder();
            }
            if (_orderBuilder.Length > 0)
            {
                _orderBuilder.Append(",");
            }
            OrderExpressionVisitor.Visit(ReplaceParameterVisitor.Replace(keySelector, keySelector.Parameters), _adapter, "DESC", _orderBuilder, true);
            return this;
        }

        public Query<T1, T2, T3> GroupBy<TK>(Expression<Func<T1, T2, T3, TK>> keySelector)
        {
            if (_groupBuilder == null)
            {
                _groupBuilder = new StringBuilder();
            }
            if (_groupBuilder.Length > 0)
            {
                _groupBuilder.Append(",");
            }
            OrderExpressionVisitor.Visit(ReplaceParameterVisitor.Replace(keySelector, keySelector.Parameters), _adapter, null, _groupBuilder, true);
            return this;
        }

        public Query<T1, T2, T3> Having(Expression<Func<T1, T2, T3, bool>> keySelector)
        {
            if (_having == null)
            {
                _having = new StringBuilder();
            }
            WhereExpressionVisitor.Visit(ReplaceParameterVisitor.Replace(ExpressionResolver.Visit(keySelector), keySelector.Parameters), _adapter, _having, Parameters, true);
            return this;
        }

        public TResult Max<TResult>(Expression<Func<T1, T2, T3, TResult>> keySelector)
        {
            BuildMaxSql(keySelector);
            string commandText = GetFunctionCommandText();
            return Connection.QueryFirstOrDefault<TResult>(commandText, Parameters);
        }

        public Task<TResult> MaxAsync<TResult>(Expression<Func<T1, T2, T3, TResult>> keySelector)
        {
            BuildMaxSql(keySelector);
            string commandText = GetFunctionCommandText();
            return Connection.QueryFirstOrDefaultAsync<TResult>(commandText, Parameters);
        }

        private void BuildMaxSql<TResult>(Expression<Func<T1, T2, T3, TResult>> keySelector)
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
            OrderExpressionVisitor.Visit(ReplaceParameterVisitor.Replace(keySelector, keySelector.Parameters), _adapter, null, _aggregateBuilder, true);
            _aggregateBuilder.Append(")");
        }

        public TResult Min<TResult>(Expression<Func<T1, T2, T3, TResult>> keySelector)
        {
            BuildMinSql(keySelector);
            string commandText = GetFunctionCommandText();
            return Connection.QueryFirstOrDefault<TResult>(commandText, Parameters);
        }

        public Task<TResult> MinAsync<TResult>(Expression<Func<T1, T2, T3, TResult>> keySelector)
        {
            BuildMinSql(keySelector);
            string commandText = GetFunctionCommandText();
            return Connection.QueryFirstOrDefaultAsync<TResult>(commandText, Parameters);
        }

        private void BuildMinSql<TResult>(Expression<Func<T1, T2, T3, TResult>> keySelector)
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
            OrderExpressionVisitor.Visit(ReplaceParameterVisitor.Replace(keySelector, keySelector.Parameters), _adapter, null, _aggregateBuilder, true);
            _aggregateBuilder.Append(")");
        }

        public TResult Sum<TResult>(Expression<Func<T1, T2, T3, TResult>> keySelector)
        {
            BuildSumSql(keySelector);
            string commandText = GetFunctionCommandText();
            return Connection.QueryFirstOrDefault<TResult>(commandText, Parameters);
        }

        public Task<TResult> SumAsync<TResult>(Expression<Func<T1, T2, T3, TResult>> keySelector)
        {
            BuildSumSql(keySelector);
            string commandText = GetFunctionCommandText();
            return Connection.QueryFirstOrDefaultAsync<TResult>(commandText, Parameters);
        }

        private void BuildSumSql<TResult>(Expression<Func<T1, T2, T3, TResult>> keySelector)
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


        private bool IsFromKeySelector(LambdaExpression lambda, out ParameterExpression p)
        {
            p = null;
            if (!(lambda.Body is ParameterExpression bodyParameter))
            {
                return false;
            }
            foreach (LambdaExpression onExpression in _onExpressions)
            {
                foreach (ParameterExpression parameter in lambda.Parameters)
                {
                    int index = lambda.Parameters.IndexOf(parameter);
                    if (index > lambda.Parameters.Count - 1)
                    {
                        continue;
                    }
                    if (parameter.Name == bodyParameter.Name)
                    {
                        p = onExpression.Parameters[index];
                        return true;
                    }
                }
            }
            return false;
        }

        public Query<T1, T2, T3> Select<T4>(Expression<Func<T1, T2, T3, T4>> selector)
        {
            if (_selectBuilder.Length > 0)
            {
                return this;
            }
            if (IsFromKeySelector(selector, out ParameterExpression p))
            {
                ParseSelect(p);
                return this;
            }
            SelectExpressionVisitor.Visit(ReplaceParameterVisitor.Replace(selector, selector.Parameters), _adapter, _selectBuilder, true);
            return this;
        }

        private void ParseSelect(ParameterExpression p)
        {
            IList<PropertyInfo> validPropertyInfos = TypeProvider.GetCanQueryProperties(p.Type);
            foreach (var property in validPropertyInfos)
            {
                _selectBuilder.Append(_adapter.GetQuoteName(p.Name)).Append(".");
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

        public Query<T1, T2, T3> Distinct()
        {
            _distinct = true;
            return this;
        }

        public string GetCommandText()
        {
            StringBuilder sqlBuilder = new StringBuilder();
            BuildSelect(sqlBuilder);
            ParseTable(sqlBuilder);
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
            if (_selectBuilder.Length == 0)
            {
                Select(DefaultSelector);
            }
            sqlBuilder.Append(_selectBuilder);
        }

        private void ParseTable(StringBuilder sqlBuilder)
        {
            sqlBuilder.Append(" FROM ");
            LambdaExpression lambda = _onExpressions[1];
            foreach (ParameterExpression e in lambda.Parameters)
            {
                int index = lambda.Parameters.IndexOf(e);
                if (index == 0)
                {
                    AddTable(sqlBuilder, e);
                    continue;
                }
                sqlBuilder.AppendFormat(" {0} ", SqlProvider.JoinTypeSqlCause[_joinTypes[index - 1]]);
                AddTable(sqlBuilder, e);
                sqlBuilder.Append(" ON ");
                OnExpressionVisitor.Visit(_onExpressions[index - 1], _adapter, sqlBuilder);
            }
        }

        private void AddTable(StringBuilder sqlBuilder, ParameterExpression parameter)
        {
            _adapter.AppendQuoteName(sqlBuilder, TypeProvider.GetTableName(parameter.Type));
            sqlBuilder.Append(" AS ").Append(_adapter.GetQuoteName(parameter.Name));
        }

        public string GetCountCommandText()
        {
            StringBuilder countBuilder = new StringBuilder();
            countBuilder.Append("SELECT COUNT(*)");
            ParseTable(countBuilder);
            BuildWhere(countBuilder);
            BuildGroup(countBuilder);
            return countBuilder.ToString();
        }

        private string GetFunctionCommandText()
        {
            StringBuilder countBuilder = new StringBuilder();
            countBuilder.Append("SELECT ").Append(_aggregateBuilder);
            ParseTable(countBuilder);
            BuildWhere(countBuilder);
            BuildGroup(countBuilder);
            return countBuilder.ToString();
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
