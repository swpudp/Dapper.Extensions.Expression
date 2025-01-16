using Dapper.Extensions.Expression.Adapters;
using System;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    internal class AddSecondsHandler : AbstractMethodCallHandler
    {
        public override string MethodName => "AddSeconds";

        public override bool IsMatch(MethodCallExpression exp)
        {
            return exp.Method.DeclaringType == typeof(DateTime);
        }

        public override void Handle(MethodCallExpression e, ISqlAdapter sqlAdapter, StringBuilder builder, DynamicParameters parameters, bool appendParameter)
        {
            sqlAdapter.DateTimeAddMethod(e, "SECOND", sqlAdapter, builder, parameters, appendParameter);
        }
    }
}
