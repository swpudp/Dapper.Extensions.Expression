using System;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dapper.Extensions.Expression.Queries.JoinQueries
{
    /// <summary>
    /// 联合查询3
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    public sealed class JoinQuery<T1, T2, T3> : AbstractQuery, IQuery
    {
        /// <summary>
        /// 默认select选择器
        /// </summary>
        private static readonly Expression<Func<T1, T2, T3, T1>> FirstSelector = (t1, t2, t3) => t1;

        /// <summary>
        /// 默认select选择器
        /// </summary>
        protected override LambdaExpression DefaultSelector => FirstSelector;

        internal JoinQuery(IDbConnection connection) : base(connection, 2)
        {
        }

        public JoinQuery<T1, T2, T3> On(JoinType joinType, Expression<Func<T1, T2, bool>> on)
        {
            SetOn(joinType, on, 0);
            return this;
        }

        public JoinQuery<T1, T2, T3> On(JoinType joinType, Expression<Func<T1, T2, T3, bool>> on)
        {
            SetOn(joinType, on, 1);
            return this;
        }

        /// <summary>
        /// 设置条件
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public JoinQuery<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> ex)
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
        public JoinQuery<T1, T2, T3> WhereIf(bool condition, Expression<Func<T1, T2, T3, bool>> ex)
        {
            base.WhereIf(condition, ex);
            return this;
        }

        public JoinQuery<T1, T2, T3> Exist<T4>(Expression<Func<T1, T2, T3, T4, bool>> where)
        {
            base.Exist(where);
            return this;
        }

        public JoinQuery<T1, T2, T3> Between<TK>(Expression<Func<T1, T2, T3, TK>> selector, TK left, TK right)
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
        public JoinQuery<T1, T2, T3> TakePage(int page, int pageSize)
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
        public JoinQuery<T1, T2, T3> Take(int pageSize)
        {
            SetPageSize(pageSize);
            return this;
        }

        public JoinQuery<T1, T2, T3> OrderBy<TK>(Expression<Func<T1, T2, T3, TK>> keySelector)
        {
            OrderByExpression(keySelector, ConstantDefined.OrderAsc);
            return this;
        }

        public JoinQuery<T1, T2, T3> OrderByDescending<TK>(Expression<Func<T1, T2, T3, TK>> keySelector)
        {
            OrderByExpression(keySelector, ConstantDefined.OrderDesc);
            return this;
        }

        public JoinQuery<T1, T2, T3> OrderBy(string propertyName)
        {
            OrderByPropertyName(propertyName, ConstantDefined.OrderAsc);
            return this;
        }

        public JoinQuery<T1, T2, T3> OrderByDescending(string propertyName)
        {
            OrderByPropertyName(propertyName, ConstantDefined.OrderDesc);
            return this;
        }

        public JoinQuery<T1, T2, T3> GroupBy<TK>(Expression<Func<T1, T2, T3, TK>> keySelector)
        {
            base.GroupBy(keySelector);
            return this;
        }

        public JoinQuery<T1, T2, T3> Having(Expression<Func<T1, T2, T3, bool>> keySelector)
        {
            base.Having(keySelector);
            return this;
        }

        public TResult Max<TResult>(Expression<Func<T1, T2, T3, TResult>> keySelector)
        {
            return base.Max<TResult>(keySelector);
        }

        public Task<TResult> MaxAsync<TResult>(Expression<Func<T1, T2, T3, TResult>> keySelector)
        {
            return base.MaxAsync<TResult>(keySelector);
        }

        public TResult Min<TResult>(Expression<Func<T1, T2, T3, TResult>> keySelector)
        {
            return base.Min<TResult>(keySelector);
        }

        public Task<TResult> MinAsync<TResult>(Expression<Func<T1, T2, T3, TResult>> keySelector)
        {
            return base.MinAsync<TResult>(keySelector);
        }

        public TResult Sum<TResult>(Expression<Func<T1, T2, T3, TResult>> keySelector)
        {
            return base.Sum<TResult>(keySelector);
        }

        public Task<TResult> SumAsync<TResult>(Expression<Func<T1, T2, T3, TResult>> keySelector)
        {
            return base.SumAsync<TResult>(keySelector);
        }

        public JoinQuery<T1, T2, T3> Select<T4>(Expression<Func<T1, T2, T3, T4>> selector)
        {
            base.Select(selector);
            return this;
        }

        public JoinQuery<T1, T2, T3> Distinct()
        {
            SetDistinct();
            return this;
        }
    }
}
