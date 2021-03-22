using System.Linq.Expressions;
using Dapper.Extensions.Expression.Core.Visitors;
using Dapper.Extensions.Expression.DbExpressions;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Extensions.Expression.Query.Visitors
{
    class GroupKeySelectorParser : ExpressionVisitor<DbExpression[]>
    {
        ScopeParameterDictionary _scopeParameters;
        StringSet _scopeTables;
        public GroupKeySelectorParser(ScopeParameterDictionary scopeParameters, StringSet scopeTables)
        {
            this._scopeParameters = scopeParameters;
            this._scopeTables = scopeTables;
        }

        public static DbExpression[] Parse(System.Linq.Expressions.Expression keySelector, ScopeParameterDictionary scopeParameters, StringSet scopeTables)
        {
            return new GroupKeySelectorParser(scopeParameters, scopeTables).Visit(keySelector);
        }

        public override DbExpression[] Visit(System.Linq.Expressions.Expression exp)
        {
            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);
                case ExpressionType.New:
                    return this.VisitNew((NewExpression)exp);
                default:
                    {
                        var dbExp = GeneralExpressionParser.Parse(exp, this._scopeParameters, this._scopeTables);
                        return new DbExpression[1] { dbExp };
                    }
            }
        }

        protected override DbExpression[] VisitLambda(LambdaExpression exp)
        {
            return this.Visit(exp.Body);
        }
        protected override DbExpression[] VisitNew(NewExpression exp)
        {
            DbExpression[] ret = new DbExpression[exp.Arguments.Count];
            for (int i = 0; i < exp.Arguments.Count; i++)
            {
                var dbExp = GeneralExpressionParser.Parse(exp.Arguments[i], this._scopeParameters, this._scopeTables);
                ret[i] = dbExp;
            }

            return ret;
        }
    }
}
