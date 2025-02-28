﻿using Dapper.Extensions.Expression.Adapters;
using Dapper.Extensions.Expression.Visitors;
using System;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extensions.Expression.MethodCalls
{
    internal class EqualsHandler : AbstractMethodCallHandler
    {
        public override string MethodName => "Equals";


        public override bool IsMatch(MethodCallExpression exp)
        {
            return exp.Method.DeclaringType == typeof(string);
        }

        public override void Handle(MethodCallExpression e, ISqlAdapter sqlAdapter, StringBuilder builder, DynamicParameters parameters, bool appendParameter)
        {
            System.Linq.Expressions.Expression right = e.Arguments[0];
            if (e.Object == null)
            {
                throw new InvalidOperationException();
            }
            System.Linq.Expressions.Expression exp = System.Linq.Expressions.Expression.Equal(e.Object, right);
            WhereExpressionVisitor.InternalVisit(exp, sqlAdapter, builder, parameters, appendParameter);
        }
    }
}
