using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BenchmarkDotNet;
using BenchmarkDotNet.Running;

namespace Dapper.Extensions.Expression.BenchmarkTest
{
    class Program
    {
        static void Main(string[] args)
        {
            List<System.Linq.Expressions.LambdaExpression> expressions = new List<System.Linq.Expressions.LambdaExpression>(10);
            for (int i = 0; i < 10; i++)
            {
                Expression<Func<Order, bool>> ex = (a) => a.Index > i;
                expressions.Add(ex);
            }
            //System.Linq.Expressions.Expression x = expressions.Aggregate((a, b) => b.And(a));

            var and = System.Linq.Expressions.Expression.AndAlso(expressions[0].Body, expressions[1].Body);


            var and2 = System.Linq.Expressions.Expression.AndAlso(and, expressions[2].Body);


            Console.WriteLine(new GenerateCommandText().CreateWhereIfFullParamSql());
            Console.WriteLine(new GenerateCommandText().CreateWhereIfEmptyParamSql());
            Console.WriteLine(new GenerateCommandText().CreateWhereIfPartialParamSql());
            //BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);
        }
    }

    public static class ExpressionExtensions
    {
        public static BinaryExpression And(this LambdaExpression left, LambdaExpression right)
        {
            var and = System.Linq.Expressions.Expression.AndAlso(left.Body, right.Body);
            //return System.Linq.Expressions.Expression.Lambda(and, left.Parameters.Single());
            return and;
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            if (left == null) return right;
            var and = System.Linq.Expressions.Expression.OrElse(left.Body, right.Body);
            return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(and, left.Parameters.Single());
        }
    }
}
