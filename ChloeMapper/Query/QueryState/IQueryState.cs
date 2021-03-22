using System;
using System.Linq.Expressions;
using Dapper.Extensions.Expression.Query.Mapping;
using Dapper.Extensions.Expression.Query.Model;
using Dapper.Extensions.Expression.Query.QueryExpressions;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Extensions.Expression.Query.QueryState
{
    public interface IQueryState
    {
        MappingData GenerateMappingData();

        QueryModel ToFromQueryModel();
        JoinQueryResult ToJoinQueryResult(JoinType joinType, LambdaExpression conditionExpression, ScopeParameterDictionary scopeParameters, StringSet scopeTables, Func<string, string> tableAliasGenerator);

        IQueryState Accept(WhereExpression exp);
        IQueryState Accept(OrderExpression exp);
        IQueryState Accept(SelectExpression exp);
        IQueryState Accept(SkipExpression exp);
        IQueryState Accept(TakeExpression exp);
        IQueryState Accept(AggregateQueryExpression exp);
        IQueryState Accept(GroupingQueryExpression exp);
        IQueryState Accept(DistinctExpression exp);
        IQueryState Accept(IncludeExpression exp);
        IQueryState Accept(IgnoreAllFiltersExpression exp);
    }
}
