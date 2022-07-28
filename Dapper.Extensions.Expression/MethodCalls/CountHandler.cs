using Dapper.Extensions.Expression.Adapters;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    internal class CountHandler : AbstractMethodCallHandler
    {
        public override string MethodName => "Count";

        public override bool IsMatch(MethodCallExpression exp)
        {
            return exp.Method.DeclaringType == typeof(Function);
        }

        public override void Handle(MethodCallExpression e, ISqlAdapter sqlAdapter, StringBuilder builder, DynamicParameters parameters, bool appendParameter)
        {
            builder.Append("Count(*) AS ");
        }
    }
}
