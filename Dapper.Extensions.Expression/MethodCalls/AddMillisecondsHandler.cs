using Dapper.Extensions.Expression.Adapters;
using System;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    internal class AddMillisecondsHandler : AbstractMethodCallHandler
    {
        public override string MethodName => "AddMilliseconds";

        public override bool IsMatch(MethodCallExpression exp)
        {
            return exp.Method.DeclaringType == typeof(DateTime);
        }

        public override void Handle(MethodCallExpression e, ISqlAdapter sqlAdapter, StringBuilder builder, DynamicParameters parameters, bool appendParameter)
        {
            sqlAdapter.DateTimeAddMethod(e, "Millisecond", sqlAdapter, builder, parameters, appendParameter);
        }
    }
}
