using Dapper.Extensions.Expression.Adapters;
using Dapper.Extensions.Expression.Utilities;
using Dapper.Extensions.Expression.Visitors;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    internal class StringContainsHandler : AbstractMethodCallHandler
    {
        public override string MethodName => "Contains";

        public override bool IsMatch(MethodCallExpression exp)
        {
            return exp.Method.DeclaringType == typeof(string);
        }

        public override void Handle(MethodCallExpression e, ISqlAdapter sqlAdapter, StringBuilder builder, DynamicParameters parameters, bool appendParameter)
        {
            WhereExpressionVisitor.InternalVisit(e.Object, sqlAdapter, builder, parameters, appendParameter);
            builder.Append(" LIKE ");
            object v = ExpressionEvaluator.Visit(e.Arguments[0]);
            WhereExpressionVisitor.AddParameter(builder, parameters, "%" + v + "%");
        }
    }
}
