using Dapper.Extensions.Expression.Adapters;
using Dapper.Extensions.Expression.Visitors;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    internal class NullOrEmptyHandler : AbstractMethodCallHandler
    {
        public override string MethodName => "IsNullOrEmpty";

        public override bool IsMatch(MethodCallExpression exp)
        {
            return exp.Method.DeclaringType == typeof(string);
        }

        public override void Handle(MethodCallExpression e, ISqlAdapter sqlAdapter, StringBuilder builder, DynamicParameters parameters, bool appendParameter)
        {
            WhereExpressionVisitor.InternalVisit(e.Arguments[0], sqlAdapter, builder, parameters, appendParameter);
            builder.Append(" IS NULL");
        }
    }
}
