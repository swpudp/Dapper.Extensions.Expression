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

        string GetCommandText();

        string GetCountCommandText();

        string GetFunctionCommandText();
    }


    public sealed class Query<T> : IQuery
    {
        /// <summary>
        /// WHERE表达式
        /// </summary>
        private IList<System.Linq.Expressions.Expression> _expressions;

        /// <summary>
        /// sql适配器
        /// </summary>
        private readonly ISqlAdapter _adapter;

        /// <summary>
        /// count的sql语句
        /// </summary>
        private string _countSql;

        /// <summary>
        /// sql语句
        /// </summary>
        private string _commandSql;

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
        /// 表达式访问
        /// </summary>
        private ExpressionVisitor _visitor;

        public Query(IDbConnection connection)
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
            if (_expressions == null)
            {
                _expressions = new List<System.Linq.Expressions.Expression>();
                _whereBuilder = new StringBuilder();
            }
            if (_visitor == null)
            {
                _visitor = new ExpressionVisitor(_adapter);
            }
            _expressions.Add(ex);
            return this;
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
        /// <param name="order"></param>
        /// <returns></returns>
        public Query<T> OrderBy(string order)
        {
            if (_orderBuilder == null)
            {
                _orderBuilder = new StringBuilder(null);
            }
            if (string.IsNullOrWhiteSpace(order))
            {
                return this;
            }
            if (_orderBuilder.Length > 0)
            {
                _orderBuilder.Append(",");
            }
            _orderBuilder.AppendFormat(" {0} ", order);
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
            IList<MemberInfo> memberInfos = OrderExpressionResolver.Visit(keySelector).ToList();
            foreach (MemberInfo memberInfo in memberInfos)
            {
                int index = memberInfos.IndexOf(memberInfo);
                _adapter.AppendColumnName(_orderBuilder, memberInfo);
                if (index < memberInfos.Count - 1)
                {
                    _orderBuilder.Append(",");
                }
            }
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
            IList<MemberInfo> memberInfos = OrderExpressionResolver.Visit(keySelector).ToList();
            foreach (MemberInfo memberInfo in memberInfos)
            {
                int index = memberInfos.IndexOf(memberInfo);
                _adapter.AppendColumnName(_orderBuilder, memberInfo);
                _orderBuilder.Append(" DESC");
                if (index < memberInfos.Count - 1)
                {
                    _orderBuilder.Append(",");
                }
            }
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
            IList<MemberInfo> memberInfos = OrderExpressionResolver.Visit(keySelector).ToList();
            foreach (MemberInfo memberInfo in memberInfos)
            {
                int index = memberInfos.IndexOf(memberInfo);
                _adapter.AppendColumnName(_groupBuilder, memberInfo);
                if (index < memberInfos.Count - 1)
                {
                    _groupBuilder.Append(",");
                }
            }
            return this;
        }

        public Query<T> Having(Expression<Func<T, bool>> keySelector)
        {
            if (_having == null)
            {
                _having = new StringBuilder();
            }
            if (_visitor == null)
            {
                _visitor = new ExpressionVisitor(_adapter);
            }
            _visitor.Visit(keySelector, _having, Parameters);
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
            MemberInfo memberInfo = OrderExpressionResolver.Visit(keySelector).First();
            _adapter.AppendColumnName(_aggregateBuilder, memberInfo);
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
            MemberInfo memberInfo = OrderExpressionResolver.Visit(keySelector).First();
            _adapter.AppendColumnName(_aggregateBuilder, memberInfo);
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
            MemberInfo memberInfo = OrderExpressionResolver.Visit(keySelector).First();
            _adapter.AppendColumnName(_aggregateBuilder, memberInfo);
            _aggregateBuilder.Append(")");
        }

        public Query<T> Select(Expression<Func<T, object>> selector)
        {
            SelectExpressionResolver.Visit(selector, _adapter, _selectBuilder);
            return this;
        }

        public Query<T> Select(Expression<Func<T, T>> selector)
        {
            SelectExpressionResolver.Visit(selector, _adapter, _selectBuilder);
            return this;
        }

        public Query<T> Distinct()
        {
            _distinct = true;
            return this;
        }

        /// <summary>
        /// 构建
        /// </summary>
        /// <returns></returns>
        private void ParseWhere()
        {
            if (_whereBuilder == null || _whereBuilder.Length > 0)
            {
                return;
            }
            foreach (System.Linq.Expressions.Expression ex in _expressions)
            {
                _visitor.Visit(ex, _whereBuilder, Parameters);
                if (_expressions.IndexOf(ex) < _expressions.Count - 1)
                {
                    _whereBuilder.Append(" AND ");
                }
            }
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
                _adapter.AppendColumnName(_selectBuilder, property);
                if (validPropertyInfos.IndexOf(property) < validPropertyInfos.Count - 1)
                {
                    _selectBuilder.Append(",");
                }
            }
        }

        public string GetCommandText()
        {
            if (!string.IsNullOrEmpty(_commandSql))
            {
                return _commandSql;
            }
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append("SELECT ");
            if (_distinct)
            {
                sqlBuilder.Append(" DISTINCT ");
            }
            ParseSelect();
            sqlBuilder.AppendFormat("{0} FROM ", _selectBuilder);
            _adapter.AppendTableName(sqlBuilder, TypeProvider.GetTableName(typeof(T)));
            BuildWhere(sqlBuilder);
            BuildGroup(sqlBuilder);
            BuildOrder(sqlBuilder);
            _adapter.AppendPage(sqlBuilder, _page, _pageSize);
            _commandSql = sqlBuilder.ToString();
            return _commandSql;
        }

        public string GetCountCommandText()
        {
            if (!string.IsNullOrEmpty(_countSql))
            {
                return _countSql;
            }
            StringBuilder countBuilder = new StringBuilder();
            countBuilder.Append("SELECT COUNT(*) FROM ");
            _adapter.AppendTableName(countBuilder, TypeProvider.GetTableName(typeof(T)));
            BuildWhere(countBuilder);
            BuildGroup(countBuilder);
            _countSql = countBuilder.ToString();
            return _countSql;
        }

        public string GetFunctionCommandText()
        {
            if (!string.IsNullOrEmpty(_countSql))
            {
                return _countSql;
            }
            StringBuilder countBuilder = new StringBuilder();
            countBuilder.Append("SELECT ").Append(_aggregateBuilder).Append(" FROM ");
            _adapter.AppendTableName(countBuilder, TypeProvider.GetTableName(typeof(T)));
            BuildWhere(countBuilder);
            BuildGroup(countBuilder);
            _countSql = countBuilder.ToString();
            return _countSql;
        }

        private void BuildWhere(StringBuilder builder)
        {
            ParseWhere();
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
