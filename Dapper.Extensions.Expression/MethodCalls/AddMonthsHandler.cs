using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    internal class AddMonthsHandler : AbstractMethodCallHandler
    {
        public override string MethodName => "AddMonths";

        public override bool IsMatch(MethodInfo methodInfo)
        {
            return methodInfo.DeclaringType == typeof(DateTime);
        }

        public override void Handle(MethodCallExpression e, ExpressionVisitor visitor, StringBuilder builder, DynamicParameters parameters)
        {
            visitor.DateTimeAddMethod(e, "MONTH", builder, parameters);
        }
    }
}
