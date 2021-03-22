using Dapper.Extensions.Expression.Adapters;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dapper.Extensions.Expression
{
    /// <summary>
    /// where表达式
    /// </summary>
    internal class WhereExpression
    {
        /// <summary>
        /// WHERE表达式
        /// </summary>
        private readonly IList<System.Linq.Expressions.Expression> _expressions = new List<System.Linq.Expressions.Expression>();

        public void Enqueue(System.Linq.Expressions.Expression exp)
        {
            _expressions.Add(exp);
        }

        ExpressionVisitor visitor;

        public WhereExpression(ISqlAdapter sqlAdapter)
        {
            visitor = new ExpressionVisitor(sqlAdapter);
        }

        public void Visit(StringBuilder sqlBuilder, DynamicParameters parameters)
        {
            if (!_expressions.Any())
            {
                return;
            }
            foreach (System.Linq.Expressions.Expression ex in _expressions)
            {
                visitor.Visit(ex, sqlBuilder, parameters);
                if (_expressions.IndexOf(ex) < _expressions.Count - 1)
                {
                    sqlBuilder.Append(" AND ");
                }
            }
        }

        public void Visit(System.Linq.Expressions.Expression e, StringBuilder sqlBuilder, DynamicParameters parameters)
        {
            if (e == null)
            {
                return;
            }
            visitor.Visit(e, sqlBuilder, parameters);
        }
    }
}
