using Dapper.Extensions.Expression.Visitors;
using System;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dapper.Extensions.Expression.Queries.JoinQueries
{
    /// <summary>
    /// 联合查询5
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    public sealed class JoinQuery<T1, T2, T3, T4, T5> : AbstractQuery, IQuery
    {
        private readonly LambdaExpression _defaultSelector;
        protected override LambdaExpression DefaultSelector => _defaultSelector;

        internal JoinQuery(IDbConnection connection, NamingPolicy namingPolicy) : base(connection, 4,namingPolicy)
        {
            Expression<Func<T1, T2, T3, T4, T5, T1>> selector = (i, j, k, l, m) => i;
            _defaultSelector = ReplaceParameterVisitor.Replace(selector, selector.Parameters);
        }

        public JoinQuery<T1, T2, T3, T4, T5> On(JoinType joinType, Expression<Func<T1, T2, bool>> on)
        {
            SetOn(joinType, on, 0);
            return this;
        }

        public JoinQuery<T1, T2, T3, T4, T5> On(JoinType joinType, Expression<Func<T1, T2, T3, bool>> on)
        {
            SetOn(joinType, on, 1);
            return this;
        }

        public JoinQuery<T1, T2, T3, T4, T5> On(JoinType joinType, Expression<Func<T1, T2, T3, T4, bool>> on)
        {
            SetOn(joinType, on, 2);
            return this;
        }

        public JoinQuery<T1, T2, T3, T4, T5> On(JoinType joinType, Expression<Func<T1, T2, T3, T4, T5, bool>> on)
        {
            SetOn(joinType, on, 3);
            return this;
        }

        /// <summary>
        /// 设置条件
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public JoinQuery<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> ex)
        {
            base.Where(ex);
            return this;
        }

        /// <summary>
        /// 带条件筛选
        /// </summary>
        /// <param name="condition">是否进入筛选</param>
        /// <param name="ex">表达式</param>
        /// <returns></returns>
        public JoinQuery<T1, T2, T3, T4, T5> WhereIf(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ex)
        {
            base.WhereIf(condition, ex);
            return this;
        }

        public JoinQuery<T1, T2, T3, T4, T5> Exist<T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> where)
        {
            base.Exist(where);
            return this;
        }

        public JoinQuery<T1, T2, T3, T4, T5> Between<TK>(Expression<Func<T1, T2, T3, T4, T5, TK>> selector, TK left, TK right)
        {
            base.Between(selector, left, right);
            return this;
        }

        /// <summary>
        /// 设置分页信息
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public JoinQuery<T1, T2, T3, T4, T5> TakePage(int page, int pageSize)
        {
            SetPage(page);
            SetPageSize(pageSize);
            return this;
        }

        /// <summary>
        /// 设置查询限制信息
        /// </summary>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public JoinQuery<T1, T2, T3, T4, T5> Take(int pageSize)
        {
            SetPageSize(pageSize);
            return this;
        }

        public JoinQuery<T1, T2, T3, T4, T5> OrderBy<TK>(Expression<Func<T1, T2, T3, T4, T5, TK>> keySelector)
        {
            base.OrderBy(keySelector);
            return this;
        }

        public JoinQuery<T1, T2, T3, T4, T5> OrderByDescending<TK>(Expression<Func<T1, T2, T3, T4, T5, TK>> keySelector)
        {
            base.OrderByDescending(keySelector);
            return this;
        }

        public JoinQuery<T1, T2, T3, T4, T5> GroupBy<TK>(Expression<Func<T1, T2, T3, T4, T5, TK>> keySelector)
        {
            base.GroupBy(keySelector);
            return this;
        }

        public JoinQuery<T1, T2, T3, T4, T5> Having(Expression<Func<T1, T2, T3, T4, T5, bool>> keySelector)
        {
            base.Having(keySelector);
            return this;
        }

        public TResult Max<TResult>(Expression<Func<T1, T2, T3, T4, T5, TResult>> keySelector)
        {
            return base.Max<TResult>(keySelector);
        }

        public Task<TResult> MaxAsync<TResult>(Expression<Func<T1, T2, T3, T4, T5, TResult>> keySelector)
        {
            return base.MaxAsync<TResult>(keySelector);
        }

        public TResult Min<TResult>(Expression<Func<T1, T2, T3, T4, T5, TResult>> keySelector)
        {
            return base.Min<TResult>(keySelector);
        }

        public Task<TResult> MinAsync<TResult>(Expression<Func<T1, T2, T3, T4, T5, TResult>> keySelector)
        {
            return base.MinAsync<TResult>(keySelector);
        }

        public TResult Sum<TResult>(Expression<Func<T1, T2, T3, T4, T5, TResult>> keySelector)
        {
            return base.Sum<TResult>(keySelector);
        }

        public Task<TResult> SumAsync<TResult>(Expression<Func<T1, T2, T3, T4, T5, TResult>> keySelector)
        {
            return base.SumAsync<TResult>(keySelector);
        }

        public JoinQuery<T1, T2, T3, T4, T5> Select<T6>(Expression<Func<T1, T2, T3, T4, T5, T6>> selector)
        {
            base.Select(selector);
            return this;
        }

        public JoinQuery<T1, T2, T3, T4, T5> Distinct()
        {
            SetDistinct();
            return this;
        }
    }
}
