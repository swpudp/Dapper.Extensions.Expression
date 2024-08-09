using Dapper.Extensions.Expression.Adapters;
using Dapper.Extensions.Expression.Visitors;
using System;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    internal class GuidParseHandler : AbstractMethodCallHandler
    {
        public override string MethodName => "Parse";
        public override bool IsMatch(MethodCallExpression exp)
        {
            return exp.Method.DeclaringType == typeof(Guid);
        }

        public override void Handle(MethodCallExpression e, ISqlAdapter sqlAdapter, StringBuilder builder, DynamicParameters parameters, bool appendParameter)
        {
            object value = Utilities.ExpressionEvaluator.Visit(e.Arguments[0]);
            WhereExpressionVisitor.AddParameter(builder, parameters, value);
        }
    }
}
