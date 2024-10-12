using Dapper.Extensions.Expression.Adapters;
using Dapper.Extensions.Expression.Visitors;
using System;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    internal class NewGuidHandler : AbstractMethodCallHandler
    {
        public override string MethodName => "NewGuid";
        public override bool IsMatch(MethodCallExpression exp)
        {
            return exp.Method.DeclaringType == typeof(Guid);
        }

        public override void Handle(MethodCallExpression e, ISqlAdapter sqlAdapter, StringBuilder builder, DynamicParameters parameters, bool appendParameter)
        {
            WhereExpressionVisitor.AddParameter(sqlAdapter, builder, parameters, Guid.NewGuid());
        }
    }
}
