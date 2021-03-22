using Dapper.Extensions.Expression.Providers;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    internal class EnumerableContainsHandler : AbstractMethodCallHandler
    {
        public override string MethodName => "Contains";

        public override bool IsMatch(MethodInfo methodInfo)
        {
            return methodInfo.IsEnumerableContains();
        }

        public override void Handle(MethodCallExpression e, ExpressionVisitor visitor, StringBuilder builder, DynamicParameters parameters)
        {
            visitor.InternalVisit(e.Arguments[1], builder, parameters);
            builder.Append(" IN (");
            IEnumerable<object> array = ExpressionEvaluator.Evaluate(e.Arguments[0]) as IEnumerable<object> ?? Enumerable.Empty<object>();
            int idx = 0;
            foreach (object v in array)
            {
                if (idx > 0)
                {
                    builder.Append(",");
                }
                idx++;
                visitor.AddParameter(builder, parameters, v);
            }
            builder.Append(")");
        }
    }
}
