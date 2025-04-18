using Dapper.Extensions.Expression.Adapters;
using Dapper.Extensions.Expression.Visitors;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    internal class MaxHandler : AbstractMethodCallHandler
    {
        public override string MethodName => "Max";

        public override bool IsMatch(MethodCallExpression exp)
        {
            return exp.Method.DeclaringType == typeof(Function);
        }

        public override void Handle(MethodCallExpression e, ISqlAdapter sqlAdapter, StringBuilder builder, DynamicParameters parameters, bool appendParameter)
        {
            builder.Append("Max(");
            SelectExpressionVisitor.Visit(e.Arguments[0], sqlAdapter, builder);
            builder.Append(") AS ");
        }
    }
}
