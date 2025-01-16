using Dapper.Extensions.Expression.Adapters;
using System;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    internal class AddMonthsHandler : AbstractMethodCallHandler
    {
        public override string MethodName => "AddMonths";

        public override bool IsMatch(MethodCallExpression exp)
        {
            return exp.Method.DeclaringType == typeof(DateTime);
        }

        public override void Handle(MethodCallExpression e, ISqlAdapter sqlAdapter, StringBuilder builder, DynamicParameters parameters, bool appendParameter)
        {
            sqlAdapter.DateTimeAddMethod(e, "MONTH", sqlAdapter, builder, parameters, appendParameter);
        }
    }
}
