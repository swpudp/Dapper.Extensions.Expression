using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Extensions.Expression.Core.Visitors
{
    class BooleanResultExpressionTransformer : ExpressionVisitor<System.Linq.Expressions.Expression>
    {
        static BooleanResultExpressionTransformer _transformer = new BooleanResultExpressionTransformer();

        static BooleanResultExpressionTransformer()
        {
            List<ExpressionType> booleanResultBinaryExpressionTypes = new List<ExpressionType>();
            booleanResultBinaryExpressionTypes.Add(ExpressionType.Equal);
            booleanResultBinaryExpressionTypes.Add(ExpressionType.NotEqual);
            booleanResultBinaryExpressionTypes.Add(ExpressionType.GreaterThan);
            booleanResultBinaryExpressionTypes.Add(ExpressionType.GreaterThanOrEqual);
            booleanResultBinaryExpressionTypes.Add(ExpressionType.LessThan);
            booleanResultBinaryExpressionTypes.Add(ExpressionType.LessThanOrEqual);
            booleanResultBinaryExpressionTypes.Add(ExpressionType.AndAlso);
            booleanResultBinaryExpressionTypes.Add(ExpressionType.OrElse);

            BooleanResultBinaryExpressionTypes = booleanResultBinaryExpressionTypes.AsReadOnly();
        }

        public static ReadOnlyCollection<ExpressionType> BooleanResultBinaryExpressionTypes { get; private set; }

        public static System.Linq.Expressions.Expression Transform(System.Linq.Expressions.Expression predicate)
        {
            return _transformer.Visit(predicate);
        }

        public override System.Linq.Expressions.Expression Visit(System.Linq.Expressions.Expression exp)
        {
            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);
                default:
                    {
                        if (exp.Type != PublicConstants.TypeOfBoolean)
                            return exp;

                        if (!BooleanResultBinaryExpressionTypes.Contains(exp.NodeType))
                            exp = System.Linq.Expressions.Expression.Equal(exp, UtilConstants.Constant_True);

                        return exp;
                    }
            }
        }

        protected override System.Linq.Expressions.Expression VisitLambda(LambdaExpression exp)
        {
            if (!BooleanResultBinaryExpressionTypes.Contains(exp.Body.NodeType))
                exp = System.Linq.Expressions.Expression.Lambda(System.Linq.Expressions.Expression.Equal(exp.Body, UtilConstants.Constant_True), exp.Parameters.ToArray());

            return exp;
        }
    }
}
