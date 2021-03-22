using System.Linq.Expressions;

namespace Dapper.Extensions.Expression.Query
{
    public class JoinQueryInfo
    {
        public JoinQueryInfo(IQuery query, JoinType joinType, LambdaExpression condition)
        {
            this.Query = query;
            this.JoinType = joinType;
            this.Condition = condition;
        }
        public IQuery Query { get; set; }
        public JoinType JoinType { get; set; }
        public LambdaExpression Condition { get; set; }
    }
}
