using Dapper.Extensions.Expression.Adapters;
using Dapper.Extensions.Expression.Utilities;
using Dapper.Extensions.Expression.Visitors;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    internal class EnumerableContainsHandler : AbstractMethodCallHandler
    {
        public override string MethodName => "Contains";

        public override bool IsMatch(MethodCallExpression methodInfo)
        {
            return methodInfo.Method.IsStatic && methodInfo.Method.DeclaringType == typeof(Enumerable) && methodInfo.Arguments.Count == 2;
        }

        public override void Handle(MethodCallExpression e, ISqlAdapter sqlAdapter, StringBuilder builder, DynamicParameters parameters, bool appendParameter)
        {
            if (!(ExpressionEvaluator.Visit(e.Arguments[0]) is IEnumerable array))
            {
                return;
            }
            List<object> values = array.Cast<object>().ToList();
            if (values.Count != 0)
            {
                WhereExpressionVisitor.InternalVisit(e.Arguments[1], sqlAdapter, builder, parameters, appendParameter);
                builder.Append(" IN (");
                foreach (object v in values)
                {
                    if (values.IndexOf(v) > 0)
                    {
                        builder.Append(',');
                    }
                    WhereExpressionVisitor.AddParameter(sqlAdapter, builder, parameters, v);
                }
                builder.Append(')');
            }
            else
            {
                builder.Append(" 1=0 ");
            }
        }
    }
}
