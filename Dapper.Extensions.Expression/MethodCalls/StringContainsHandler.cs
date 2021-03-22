using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    internal class StringContainsHandler : AbstractMethodCallHandler
    {
        public override string MethodName => "Contains";

        public override bool IsMatch(MethodInfo methodInfo)
        {
            return methodInfo.DeclaringType == typeof(string);
        }

        public override void Handle(MethodCallExpression e, ExpressionVisitor visitor, StringBuilder builder, DynamicParameters parameters)
        {
            visitor.InternalVisit(e.Object, builder, parameters);
            builder.Append(" LIKE ");
            object v = ExpressionEvaluator.Evaluate(e.Arguments[0]);
            visitor.AddParameter(builder,parameters, "%" + v + "%");
        }
    }
}
