using Dapper.Extensions.Expression.Adapters;
using Dapper.Extensions.Expression.Visitors;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    internal class ToStringHandler : AbstractMethodCallHandler
    {
        public override string MethodName => "ToString";

        public override bool IsMatch(MethodCallExpression exp)
        {
            return exp.Method.DeclaringType == typeof(string);
        }

        public override void Handle(MethodCallExpression e, ISqlAdapter sqlAdapter, StringBuilder builder, DynamicParameters parameters, bool appendParameter)
        {
            if (e.Object == null)
            {
                return;
            }
            if (e.Object.Type == ConstantDefined.TypeOfString)
            {
                WhereExpressionVisitor.InternalVisit(e.Object, sqlAdapter, builder, parameters, appendParameter);
                return;
            }
            UnaryExpression c = System.Linq.Expressions.Expression.Convert(e.Object, ConstantDefined.TypeOfString);
            WhereExpressionVisitor.InternalVisit(c, sqlAdapter, builder, parameters, appendParameter);
        }
    }
}
