using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Dapper.Extensions.Expression.Query
{
    static class QueryHelper
    {
        public static Expression<TDelegate> ComposePredicate<TDelegate>(List<LambdaExpression> filterPredicates, System.Linq.Expressions.Expression[] expressionSubstitutes, ParameterExpression parameter)
        {
            System.Linq.Expressions.Expression predicateBody = null;
            foreach (LambdaExpression filterPredicate in filterPredicates)
            {
                var body = JoinQueryParameterExpressionReplacer.Replace(filterPredicate, expressionSubstitutes, parameter).Body;
                if (predicateBody == null)
                {
                    predicateBody = body;
                }
                else
                {
                    predicateBody = System.Linq.Expressions.Expression.AndAlso(predicateBody, body);
                }
            }

            Expression<TDelegate> predicate = System.Linq.Expressions.Expression.Lambda<TDelegate>(predicateBody, parameter);

            return predicate;
        }

        public static System.Linq.Expressions.Expression[] MakeExpressionSubstitutes(Type joinResultType, ParameterExpression parameter)
        {
            int joinResultTypeGenericArgumentCount = joinResultType.GetGenericArguments().Length;
            System.Linq.Expressions.Expression[] expressionSubstitutes = new System.Linq.Expressions.Expression[joinResultTypeGenericArgumentCount];
            for (int i = 0; i < joinResultTypeGenericArgumentCount; i++)
            {
                expressionSubstitutes[i] = System.Linq.Expressions.Expression.MakeMemberAccess(parameter, joinResultType.GetProperty("Item" + (i + 1).ToString()));
            }

            return expressionSubstitutes;
        }
    }
}
