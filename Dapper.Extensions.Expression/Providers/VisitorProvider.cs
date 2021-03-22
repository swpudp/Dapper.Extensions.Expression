using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extensions.Expression.Providers
{
    internal static class VisitorProvider
    {
        private static readonly ConcurrentDictionary<ExpressionType, Action<System.Linq.Expressions.Expression, StringBuilder, DynamicParameters>> TypeTableName = new ConcurrentDictionary<ExpressionType, Action<System.Linq.Expressions.Expression, StringBuilder, DynamicParameters>>();

        public static Action<System.Linq.Expressions.Expression, StringBuilder, DynamicParameters> GetVisitor(ExpressionType nodeType)
        {
            if (TypeTableName.TryGetValue(nodeType, out Action<System.Linq.Expressions.Expression, StringBuilder, DynamicParameters> visitor))
            {
                return visitor;
            }
            throw new NotSupportedException();
        }

        public static void AddVisitor(ExpressionType nodeType, Action<System.Linq.Expressions.Expression, StringBuilder, DynamicParameters> action)
        {
            if (TypeTableName.ContainsKey(nodeType))
            {
                return;
            }
            TypeTableName.TryAdd(nodeType, action);
        }

        public static bool ContainsKey()
        {
            return TypeTableName.Keys.Any();
        }
    }
}
