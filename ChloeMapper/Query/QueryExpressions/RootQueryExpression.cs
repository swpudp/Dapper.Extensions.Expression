using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Dapper.Extensions.Expression.Query.QueryExpressions
{
    public class RootQueryExpression : QueryExpression
    {
        public RootQueryExpression(Type entityType, string explicitTable, LockType @lock)
            : base(QueryExpressionType.Root, entityType, null)
        {
            this.ExplicitTable = explicitTable;
            this.Lock = @lock;
        }
        public string ExplicitTable { get; private set; }
        public LockType Lock { get; private set; }
        public List<LambdaExpression> ContextFilters { get; private set; } = new List<LambdaExpression>();
        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
