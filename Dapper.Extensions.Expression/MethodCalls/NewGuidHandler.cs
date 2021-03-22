using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    internal class NewGuidHandler : AbstractMethodCallHandler
    {
        public override string MethodName => "NewGuid";
        public override bool IsMatch(MethodInfo methodInfo)
        {
            return methodInfo.DeclaringType == typeof(Guid);
        }

        public override void Handle(MethodCallExpression e, ExpressionVisitor visitor, StringBuilder builder, DynamicParameters parameters)
        {
            builder.AppendFormat(" UUID() ");
        }
    }
}
