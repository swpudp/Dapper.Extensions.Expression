using Dapper.Extensions.Expression.Adapters;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    /// <summary>
    /// 处理方法调用
    /// </summary>
    internal abstract class AbstractMethodCallHandler
    {
        /// <summary>
        /// 方法名称
        /// </summary>
        public abstract string MethodName { get; }

        /// <summary>
        /// 是否满足条件
        /// </summary>
        public abstract bool IsMatch(MethodCallExpression exp);

        /// <summary>
        /// 处理方法调用
        /// </summary>
        public abstract void Handle(MethodCallExpression e, ISqlAdapter sqlAdapter, StringBuilder builder, DynamicParameters parameters, bool appendParameter);
    }
}
