using Dapper.Extensions.Expression.Adapters;
using System;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    internal class AddHoursHandler : AbstractMethodCallHandler
    {
        public override string MethodName => "AddHours";

        public override bool IsMatch(MethodCallExpression exp)
        {
            return exp.Method.DeclaringType == typeof(DateTime);
        }

        public override void Handle(MethodCallExpression e, ISqlAdapter sqlAdapter, StringBuilder builder, DynamicParameters parameters, bool appendParameter)
        {
            sqlAdapter.DateTimeAddMethod(e, "HOUR", builder, parameters, appendParameter);
        }
    }
}
