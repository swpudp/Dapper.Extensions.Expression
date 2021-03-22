using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    internal class EqualsHandler : AbstractMethodCallHandler
    {
        public override string MethodName => "Equals";


        public override bool IsMatch(MethodInfo methodInfo)
        {
            return methodInfo.DeclaringType == typeof(string);
        }

        public override void Handle(MethodCallExpression e, ExpressionVisitor visitor, StringBuilder builder, DynamicParameters parameters)
        {
            System.Linq.Expressions.Expression right = e.Arguments[0];
            if (e.Object == null)
            {
                throw new InvalidOperationException();
            }
            System.Linq.Expressions.Expression exp = System.Linq.Expressions.Expression.Equal(e.Object, right);
            visitor.InternalVisit(exp, builder, parameters);
        }
    }
}
