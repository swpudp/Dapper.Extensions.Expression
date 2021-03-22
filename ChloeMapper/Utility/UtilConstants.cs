using System;
using System.Linq.Expressions;

namespace Dapper.Extensions.Expression.Utility
{
    static class UtilConstants
    {
        public const string DefaultTableAlias = "T";
        public const string DefaultColumnAlias = "C";

        public static readonly ConstantExpression Constant_Null_String = System.Linq.Expressions.Expression.Constant(null, typeof(string));
        public static readonly ConstantExpression Constant_Empty_String = System.Linq.Expressions.Expression.Constant(string.Empty);
        public static readonly ConstantExpression Constant_Null_Boolean = System.Linq.Expressions.Expression.Constant(null, typeof(Boolean?));
        public static readonly ConstantExpression Constant_True = System.Linq.Expressions.Expression.Constant(true);
        public static readonly ConstantExpression Constant_False = System.Linq.Expressions.Expression.Constant(false);
        public static readonly UnaryExpression Convert_TrueToNullable = System.Linq.Expressions.Expression.Convert(System.Linq.Expressions.Expression.Constant(true), typeof(Boolean?));
        public static readonly UnaryExpression Convert_FalseToNullable = System.Linq.Expressions.Expression.Convert(System.Linq.Expressions.Expression.Constant(false), typeof(Boolean?));
    }
}
