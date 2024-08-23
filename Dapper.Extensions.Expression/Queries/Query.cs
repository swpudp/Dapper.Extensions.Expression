using System;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dapper.Extensions.Expression.Queries
{
    public sealed class Query<T> : AbstractQuery, IQuery
    {
        /// <summary>
        /// 默认select选择器
        /// </summary>
        private static readonly Expression<Func<T, T>> FirstSelector = t1 => t1;

        /// <summary>
        /// 默认select选择
        /// </summary>
        protected override LambdaExpression DefaultSelector => FirstSelector;

        internal Query(IDbConnection connection) : base(connection, 0) { }

        /// <summary>
        /// 设置条件
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public Query<T> Where(Expression<Func<T, bool>> ex)
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
        public Query<T> WhereIf(bool condition, Expression<Func<T, bool>> ex)
        {
            base.WhereIf(condition, ex);
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
            SetPage(page);
            SetPageSize(pageSize);
            return this;
        }

        /// <summary>
        /// 设置查询限制信息
        /// </summary>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public Query<T> Take(int pageSize)
        {
            SetPageSize(pageSize);
            return this;
        }

        public Query<T> OrderBy<TK>(Expression<Func<T, TK>> keySelector)
        {
            OrderByExpression(keySelector, ConstantDefined.OrderAsc);
            return this;
        }

        public Query<T> OrderByDescending<TK>(Expression<Func<T, TK>> keySelector)
        {
            OrderByExpression(keySelector, ConstantDefined.OrderDesc);
            return this;
        }

        public Query<T> OrderBy(string propertyName)
        {
            OrderByPropertyName(propertyName, ConstantDefined.OrderAsc);
            return this;
        }

        public Query<T> OrderByDescending(string propertyName)
        {
            OrderByPropertyName(propertyName, ConstantDefined.OrderDesc);
            return this;
        }

        public Query<T> GroupBy<TK>(Expression<Func<T, TK>> keySelector)
        {
            base.GroupBy(keySelector);
            return this;
        }

        public Query<T> Having(Expression<Func<T, bool>> keySelector)
        {
            base.Having(keySelector);
            return this;
        }

        public TResult Max<TResult>(Expression<Func<T, TResult>> keySelector)
        {
            return base.Max<TResult>(keySelector);
        }

        public Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> keySelector)
        {
            return base.MaxAsync<TResult>(keySelector);
        }

        public TResult Min<TResult>(Expression<Func<T, TResult>> keySelector)
        {
            return base.Min<TResult>(keySelector);
        }

        public Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> keySelector)
        {
            return base.MinAsync<TResult>(keySelector);
        }

        public TResult Sum<TResult>(Expression<Func<T, TResult>> keySelector)
        {
            return base.Sum<TResult>(keySelector);
        }

        public Task<TResult> SumAsync<TResult>(Expression<Func<T, TResult>> keySelector)
        {
            return base.SumAsync<TResult>(keySelector);
        }

        public Query<T> Select(Expression<Func<T, object>> selector)
        {
            base.Select(selector);
            return this;
        }

        public Query<T> Select(Expression<Func<T, T>> selector)
        {
            base.Select(selector);
            return this;
        }

        public Query<T> Distinct()
        {
            SetDistinct();
            return this;
        }

        public Query<T> Exist<T1>(Expression<Func<T, T1, bool>> where)
        {
            base.Exist(where);
            return this;
        }

        public Query<T> Between<TK>(Expression<Func<T, TK>> selector, TK left, TK right)
        {
            base.Between(selector, left, right);
            return this;
        }
    }
}
