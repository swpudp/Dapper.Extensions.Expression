using System;
using System.Linq.Expressions;
using Dapper.Extensions.Expression.Core.Visitors;

namespace Dapper.Extensions.Expression.Entity
{
    class PropertyNameExtractor : ExpressionVisitor<string>
    {
        static readonly PropertyNameExtractor _extractor = new PropertyNameExtractor();
        PropertyNameExtractor()
        {
        }
        public static string Extract(System.Linq.Expressions.Expression exp)
        {
            return _extractor.Visit(exp);
        }
        public override string Visit(System.Linq.Expressions.Expression exp)
        {
            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);
                case ExpressionType.MemberAccess:
                    return this.VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.Convert:
                    return this.VisitUnary_Convert((UnaryExpression)exp);
                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }
        protected override string VisitLambda(LambdaExpression exp)
        {
            return this.Visit(exp.Body);
        }
        protected override string VisitMemberAccess(MemberExpression exp)
        {
            return exp.Member.Name;
        }
        protected override string VisitUnary_Convert(UnaryExpression exp)
        {
            return this.Visit(exp.Operand);
        }
    }
}
