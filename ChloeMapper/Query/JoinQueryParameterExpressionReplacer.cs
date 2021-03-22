using System;
using System.Linq.Expressions;

namespace Dapper.Extensions.Expression.Query
{
    class JoinQueryParameterExpressionReplacer : ExpressionVisitor
    {
        LambdaExpression _lambda;
        System.Linq.Expressions.Expression[] _expressionSubstitutes;
        ParameterExpression _newParameterExpression;

        JoinQueryParameterExpressionReplacer(LambdaExpression lambda, System.Linq.Expressions.Expression[] expressionSubstitutes, ParameterExpression newParameterExpression)
        {
            this._lambda = lambda;
            this._expressionSubstitutes = expressionSubstitutes;
            this._newParameterExpression = newParameterExpression;
        }

        public static LambdaExpression Replace(LambdaExpression lambda, System.Linq.Expressions.Expression[] expressionSubstitutes, ParameterExpression newParameterExpression)
        {
            LambdaExpression ret = new JoinQueryParameterExpressionReplacer(lambda, expressionSubstitutes, newParameterExpression).Replace();
            return ret;
        }

        LambdaExpression Replace()
        {
            System.Linq.Expressions.Expression lambdaBody = this._lambda.Body;
            System.Linq.Expressions.Expression newBody = this.Visit(lambdaBody);

            Type delegateType = typeof(Func<,>).MakeGenericType(this._newParameterExpression.Type, lambdaBody.Type);
            LambdaExpression newLambda = System.Linq.Expressions.Expression.Lambda(delegateType, newBody, this._newParameterExpression);
            return newLambda;
        }

        protected override System.Linq.Expressions.Expression VisitParameter(ParameterExpression parameter)
        {
            int parameterIndex = this._lambda.Parameters.IndexOf(parameter);
            if (parameterIndex == -1)
            {
                return parameter;
            }

            return this._expressionSubstitutes[parameterIndex];
        }
    }
}
