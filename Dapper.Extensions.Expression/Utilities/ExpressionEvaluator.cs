using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dapper.Extensions.Expression.Extensions;

namespace Dapper.Extensions.Expression.Utilities
{
    internal static class ExpressionEvaluator
    {
        internal static object Visit(System.Linq.Expressions.Expression exp)
        {
            if (exp == null)
            {
                return default;
            }
            switch (exp.NodeType)
            {
                case ExpressionType.Not:
                    return VisitUnaryNot((UnaryExpression)exp);
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    return VisitUnaryConvert((UnaryExpression)exp);
                case ExpressionType.Quote:
                    return VisitUnaryQuote((UnaryExpression)exp);
                //case ExpressionType.Negate:
                //case ExpressionType.NegateChecked:
                //    return VisitUnary_Negate((UnaryExpression)exp);
                //case ExpressionType.ArrayLength:
                //case ExpressionType.TypeAs:
                //    return VisitUnary((UnaryExpression)exp);
                //case ExpressionType.Add:
                //case ExpressionType.AddChecked:
                //    return VisitBinary_Add((BinaryExpression)exp);
                //case ExpressionType.Subtract:
                //case ExpressionType.SubtractChecked:
                //    return VisitBinary_Subtract((BinaryExpression)exp);
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return VisitBinaryMultiply((BinaryExpression)exp);
                //case ExpressionType.Divide:
                //    return VisitBinary_Divide((BinaryExpression)exp);
                //case ExpressionType.Modulo:
                //    return VisitBinary_Modulo((BinaryExpression)exp);
                //case ExpressionType.And:
                //    return VisitBinary_And((BinaryExpression)exp);
                //case ExpressionType.AndAlso:
                //    return VisitBinary_AndAlso((BinaryExpression)exp);
                //case ExpressionType.Or:
                //    return VisitBinary_Or((BinaryExpression)exp);
                //case ExpressionType.OrElse:
                //    return VisitBinary_OrElse((BinaryExpression)exp);
                //case ExpressionType.LessThan:
                //    return VisitBinary_LessThan((BinaryExpression)exp);
                //case ExpressionType.LessThanOrEqual:
                //    return VisitBinary_LessThanOrEqual((BinaryExpression)exp);
                //case ExpressionType.GreaterThan:
                //    return VisitBinary_GreaterThan((BinaryExpression)exp);
                //case ExpressionType.GreaterThanOrEqual:
                //    return VisitBinary_GreaterThanOrEqual((BinaryExpression)exp);
                //case ExpressionType.Equal:
                //    return VisitBinary_Equal((BinaryExpression)exp);
                //case ExpressionType.NotEqual:
                //    return VisitBinary_NotEqual((BinaryExpression)exp);
                //case ExpressionType.Coalesce:
                //return VisitBinary_Coalesce((BinaryExpression)exp);
                //case ExpressionType.ArrayIndex:
                //case ExpressionType.RightShift:
                //case ExpressionType.LeftShift:
                //case ExpressionType.ExclusiveOr:
                //return VisitBinary((BinaryExpression)exp);
                //case ExpressionType.Lambda:
                //return VisitLambda((LambdaExpression)exp);
                //case ExpressionType.TypeIs:
                //    return this.VisitTypeIs((TypeBinaryExpression)exp);
                //case ExpressionType.Conditional:
                //return VisitConditional((ConditionalExpression)exp);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)exp);
                //case ExpressionType.Parameter:
                //return VisitParameter((ParameterExpression)exp);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)exp);
                case ExpressionType.New:
                    return VisitNew((NewExpression)exp);
                case ExpressionType.NewArrayInit:
                    //case ExpressionType.NewArrayBounds:
                    return VisitNewArray((NewArrayExpression)exp);
                //case ExpressionType.Invoke:
                //    return this.VisitInvocation((InvocationExpression)exp);
                case ExpressionType.MemberInit:
                    return VisitMemberInit((MemberInitExpression)exp);
                case ExpressionType.ListInit:
                    return VisitListInit((ListInitExpression)exp);
                default:
                    throw new NotSupportedException($"Unhandled expression type: '{exp.NodeType}'");
            }
        }

        private static object VisitMemberAccess(MemberExpression exp)
        {
            object instance = null;
            if (exp.Expression != null)
            {
                instance = Visit(exp.Expression);

                if (instance == null)
                {
                    if (exp.Member.Name == "HasValue" && exp.Member.DeclaringType.IsNullable())
                    {
                        return false;
                    }
                    throw new NullReferenceException($"There is an object reference not set to an instance in expression tree. Associated expression: '{exp.Expression}'.");
                }
            }

            return exp.Member.GetValue(instance);
        }

        internal static System.Linq.Expressions.Expression MakeExpression(System.Linq.Expressions.Expression exp)
        {
            object value = Visit(exp);
            return System.Linq.Expressions.Expression.Constant(value);
        }

        private static object VisitUnaryNot(UnaryExpression exp)
        {
            object operandValue = Visit(exp.Operand);

            return (bool)operandValue != true;
        }

        private static object VisitBinaryMultiply(BinaryExpression exp)
        {
            object leftVal = Visit(exp.Left);

            object rightVal = Visit(exp.Right);

            System.Linq.Expressions.Expression multiplyExpr = System.Linq.Expressions.Expression.Multiply(System.Linq.Expressions.Expression.Constant(leftVal), System.Linq.Expressions.Expression.Constant(rightVal));


            Delegate action = System.Linq.Expressions.Expression.Lambda(exp.Type, multiplyExpr).Compile();

            object v = action.DynamicInvoke();

            return v;
        }

        private static object VisitUnaryConvert(UnaryExpression exp)
        {
            object operandValue = Visit(exp.Operand);

            //(int)null
            if (operandValue == null)
            {
                //(int)null
                if (exp.Type.IsValueType && !exp.Type.IsNullable())
                    throw new NullReferenceException();

                return null;
            }

            Type operandValueType = operandValue.GetType();

            if (exp.Type == operandValueType || exp.Type.IsAssignableFrom(operandValueType))
            {
                return operandValue;
            }

            if (exp.Type.IsNullable(out Type underlyingType))
            {
                //(int?)int
                if (underlyingType == operandValueType)
                {
                    ConstructorInfo constructor = exp.Type.GetConstructor(new[] { operandValueType });
                    object val = constructor?.Invoke(new[] { operandValue });
                    return val;
                }
                //如果不等，则诸如：(long?)int / (long?)int?  -->  (long?)((long)int) / (long?)((long)int?)
                UnaryExpression c = System.Linq.Expressions.Expression.MakeUnary(ExpressionType.Convert, System.Linq.Expressions.Expression.Constant(operandValue), underlyingType);
                UnaryExpression cc = System.Linq.Expressions.Expression.MakeUnary(ExpressionType.Convert, c, exp.Type);
                return Visit(cc);
            }

            //(int)int?
            if (operandValueType.IsNullable(out underlyingType))
            {
                if (underlyingType == exp.Type)
                {
                    PropertyInfo pro = operandValueType.GetProperty("Value");
                    object val = pro?.GetValue(operandValue, null);
                    return val;
                }
                //如果不等，则诸如：(long)int?  -->  (long)((long)int)
                UnaryExpression c = System.Linq.Expressions.Expression.MakeUnary(ExpressionType.Convert, System.Linq.Expressions.Expression.Constant(operandValue), underlyingType);
                UnaryExpression cc = System.Linq.Expressions.Expression.MakeUnary(ExpressionType.Convert, c, exp.Type);
                return Visit(cc);
            }

            if (exp.Type.IsEnum)
            {
                return Enum.ToObject(exp.Type, operandValue);
            }

            //(long)int
            if (operandValue is IConvertible)
            {
                return Convert.ChangeType(operandValue, exp.Type);
            }

            throw new NotSupportedException($"Does not support the type '{operandValueType.FullName}' converted to type '{exp.Type.FullName}'.");
        }
        private static object VisitUnaryQuote(UnaryExpression exp)
        {
            System.Linq.Expressions.Expression e = exp.StripQuotes();
            return e;
        }
        private static object VisitConstant(ConstantExpression exp)
        {
            return exp.Value;
        }

        internal static object VisitParameter(ParameterExpression exp)
        {
            return exp;
        }

        private static object VisitMethodCall(MethodCallExpression exp)
        {
            object instance = null;
            if (exp.Object != null)
            {
                instance = Visit(exp.Object);

                if (instance == null)
                {
                    throw new NullReferenceException($"There is an object reference not set to an instance in expression tree. Associated expression: '{exp.Object}'.");
                }
            }

            object[] arguments = exp.Arguments.Select(Visit).ToArray();

            return exp.Method.Invoke(instance, arguments);
        }
        private static object VisitNew(NewExpression exp)
        {
            object[] arguments = exp.Arguments.Select(Visit).ToArray();

            return exp.Constructor.Invoke(arguments);
        }
        private static object VisitNewArray(NewArrayExpression exp)
        {
            Array arr = Array.CreateInstance(exp.Type.GetElementType(), exp.Expressions.Count);
            for (int i = 0; i < exp.Expressions.Count; i++)
            {
                System.Linq.Expressions.Expression e = exp.Expressions[i];
                arr.SetValue(Visit(e), i);
            }

            return arr;
        }
        private static object VisitMemberInit(MemberInitExpression exp)
        {
            object instance = Visit(exp.NewExpression);

            foreach (MemberBinding binding in exp.Bindings)
            {
                if (binding.BindingType != MemberBindingType.Assignment)
                {
                    throw new NotSupportedException();
                }

                MemberAssignment memberAssignment = (MemberAssignment)binding;
                MemberInfo member = memberAssignment.Member;

                member.SetMemberValue(instance, Visit(memberAssignment.Expression));
            }

            return instance;
        }
        private static object VisitListInit(ListInitExpression exp)
        {
            object instance = Visit(exp.NewExpression);

            foreach (ElementInit initializer in exp.Initializers)
            {
                foreach (System.Linq.Expressions.Expression argument in initializer.Arguments)
                {
                    initializer.AddMethod.Invoke(instance, Visit(argument));
                }
            }

            return instance;
        }
    }
}
