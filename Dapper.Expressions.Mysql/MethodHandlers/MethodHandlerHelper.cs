using Dapper.Extensions.Expression.DbExpressions;
using System;
using Dapper.Extensions.Expression.Extensions;

namespace Dapper.Expressions.Mysql.MethodHandlers
{
    class MethodHandlerHelper
    {
        public static void EnsureTrimCharArgumentIsSpaces(DbExpression exp)
        {
            if (!exp.IsEvaluable())
                throw new NotSupportedException();

            var arg = exp.Evaluate();
            if (arg == null)
                throw new ArgumentNullException();

            var chars = arg as char[];
            if (chars.Length != 1 || chars[0] != ' ')
            {
                throw new NotSupportedException();
            }
        }
    }
}
