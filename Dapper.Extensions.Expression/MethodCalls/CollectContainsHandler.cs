using Dapper.Extensions.Expression.Adapters;
using Dapper.Extensions.Expression.Utilities;
using Dapper.Extensions.Expression.Visitors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    internal class CollectContainsHandler : AbstractMethodCallHandler
    {
        public override string MethodName => "Contains";

        public override bool IsMatch(MethodCallExpression methodInfo)
        {
            Type declaringType = methodInfo.Method.DeclaringType;
            bool isAssign = typeof(IList).IsAssignableFrom(declaringType);
            bool isGeneric = declaringType != null && declaringType.IsGenericType && typeof(ICollection<>).MakeGenericType(declaringType.GetGenericArguments()).IsAssignableFrom(declaringType);
            return isAssign || isGeneric;
        }

        public override void Handle(MethodCallExpression e, ISqlAdapter sqlAdapter, StringBuilder builder, DynamicParameters parameters,
            bool appendParameter)
        {
            if (!(ExpressionEvaluator.Visit(e.Object) is IEnumerable array))
            {
                return;
            }
            IList<object> values = array.Cast<object>().ToList();
            if (values.Any())
            {
                WhereExpressionVisitor.InternalVisit(e.Arguments[0], sqlAdapter, builder, parameters, appendParameter);
                builder.Append(" IN (");
                int idx = 0;
                foreach (object v in values)
                {
                    if (idx > 0)
                    {
                        builder.Append(',');
                    }
                    idx++;
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
