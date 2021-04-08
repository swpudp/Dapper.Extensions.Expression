using Dapper.Extensions.Expression.Adapters;
using Dapper.Extensions.Expression.Visitors;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    /// <summary>
    /// 字符串比较
    /// </summary>
    internal class CompareOrdinalHandler : AbstractMethodCallHandler
    {
        /// <summary>
        /// 方法名称
        /// </summary>
        public override string MethodName => "CompareOrdinal";

        public override bool IsMatch(MethodCallExpression exp)
        {
            return exp.Method.DeclaringType == ConstantDefined.TypeOfString && exp.Method.ReturnType == ConstantDefined.TypeOfInt32;
        }

        public override void Handle(MethodCallExpression e, ISqlAdapter sqlAdapter, StringBuilder builder, DynamicParameters parameters, bool appendParameter)
        {
            WhereExpressionVisitor.InternalVisit(e.Arguments[0], sqlAdapter, builder, parameters, appendParameter);
            builder.Append(">");
            WhereExpressionVisitor.InternalVisit(e.Arguments[1], sqlAdapter, builder, parameters, appendParameter);
        }
    }
}
