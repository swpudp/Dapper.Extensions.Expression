using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Dapper.Extensions.Expression.Query.QueryExpressions
{
    public class AggregateQueryExpression : QueryExpression
    {
        MethodInfo _method;
        ReadOnlyCollection<System.Linq.Expressions.Expression> _arguments;

        public AggregateQueryExpression(QueryExpression prevExpression, MethodInfo method, IList<System.Linq.Expressions.Expression> arguments)
            : base(QueryExpressionType.Aggregate, method.ReturnType, prevExpression)
        {
            this._method = method;
            this._arguments = new ReadOnlyCollection<System.Linq.Expressions.Expression>(arguments);
        }

        public MethodInfo Method { get { return this._method; } }
        public ReadOnlyCollection<System.Linq.Expressions.Expression> Arguments { get { return this._arguments; } }


        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
