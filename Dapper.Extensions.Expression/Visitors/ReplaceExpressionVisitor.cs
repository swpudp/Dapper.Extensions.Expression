using Dapper.Extensions.Expression.Providers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Dapper.Extensions.Expression.Visitors
{
    [Obsolete("unnecessary", true)]
    internal class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private static readonly IDictionary<RuntimeTypeHandle, IDictionary<int, ParameterExpression>> NewExpressions = new ConcurrentDictionary<RuntimeTypeHandle, IDictionary<int, ParameterExpression>>();
        private readonly ReadOnlyCollection<ParameterExpression> _parameters;
        private readonly bool _onlyReplaceParameters;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="parameters">参数列表</param>
        /// <param name="onlyReplaceParameters">是否仅替换参数</param>
        internal ReplaceExpressionVisitor(ReadOnlyCollection<ParameterExpression> parameters, bool onlyReplaceParameters)
        {
            _parameters = parameters;
            _onlyReplaceParameters = onlyReplaceParameters;
        }

        protected override System.Linq.Expressions.Expression VisitMember(MemberExpression node)
        {
            if (_onlyReplaceParameters || node.Type != ConstantDefined.TypeOfBoolean)
            {
                return base.VisitMember(node);
            }
            System.Linq.Expressions.Expression left;
            ConstantExpression right;
            ExpressionType type;
            switch (node.Member.Name)
            {
                case ConstantDefined.MemberNameHasValue:
                    right = TypeProvider.GetNullExpression(node.Expression.Type);
                    type = ExpressionType.NotEqual;
                    left = node.Expression;
                    break;
                case ConstantDefined.MemberNameValue:
                    right = TypeProvider.GetTrueExpression(node.Expression.Type);
                    type = ExpressionType.Equal;
                    left = node.Expression;
                    break;
                default:
                    right = ConstantDefined.BooleanTrue;
                    type = ExpressionType.Equal;
                    left = node;
                    break;
            }
            BinaryExpression mb = System.Linq.Expressions.Expression.MakeBinary(type, left, right);
            return base.Visit(mb);
        }

        protected override System.Linq.Expressions.Expression VisitUnary(UnaryExpression node)
        {
            if (_onlyReplaceParameters || !(node.Operand is MemberExpression um))
            {
                return base.VisitUnary(node);
            }
            System.Linq.Expressions.Expression left;
            ConstantExpression right;
            if (um.Expression == null || um.Expression.NodeType == ExpressionType.Parameter)
            {
                left = um;
                right = ConstantDefined.BooleanFalse;
            }
            else
            {
                left = um.Expression;
                right = um.Member.Name == ConstantDefined.MemberNameHasValue ? TypeProvider.GetNullExpression(um.Expression.Type) : TypeProvider.GetFalseExpression(um.Expression.Type);
            }
            BinaryExpression ub = System.Linq.Expressions.Expression.Equal(left, right);
            return base.Visit(ub);
        }

        protected override System.Linq.Expressions.Expression VisitParameter(ParameterExpression node)
        {
            if (!NewExpressions.TryGetValue(node.Type.TypeHandle, out IDictionary<int, ParameterExpression> typeParameterExpressions))
            {
                typeParameterExpressions = new ConcurrentDictionary<int, ParameterExpression>();
            }
            int parameterIndex = _parameters.IndexOf(node);
            if (typeParameterExpressions.TryGetValue(parameterIndex, out ParameterExpression p))
            {
                return base.VisitParameter(p);
            }
            p = System.Linq.Expressions.Expression.Parameter(node.Type, "t" + (parameterIndex + 1));
            typeParameterExpressions[parameterIndex] = p;
            NewExpressions[node.Type.TypeHandle] = typeParameterExpressions;
            return base.VisitParameter(p);
        }
    }
}
