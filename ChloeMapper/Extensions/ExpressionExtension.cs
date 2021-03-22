using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Dapper.Extensions.Expression.Core.Visitors;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Extensions.Expression.Extensions
{
    public static class ExpressionExtension
    {
        public static BinaryExpression Assign(MemberInfo propertyOrField, System.Linq.Expressions.Expression instance, System.Linq.Expressions.Expression value)
        {
            var member = MakeMemberExpression(propertyOrField, instance);
            var setValue = System.Linq.Expressions.Expression.Assign(member, value);
            return setValue;
        }

        public static MemberExpression MakeMemberExpression(MemberInfo propertyOrField, System.Linq.Expressions.Expression instance)
        {
            if (propertyOrField.MemberType == MemberTypes.Property)
            {
                var prop = System.Linq.Expressions.Expression.Property(instance, (PropertyInfo)propertyOrField);
                return prop;
            }

            if (propertyOrField.MemberType == MemberTypes.Field)
            {
                var field = System.Linq.Expressions.Expression.Field(instance, (FieldInfo)propertyOrField);
                return field;
            }

            throw new ArgumentException();
        }

        public static LambdaExpression And(this LambdaExpression a, LambdaExpression b)
        {
            if (a == null)
                return b;
            if (b == null)
                return a;

            Type rootType = a.Parameters[0].Type;
            var memberParam = System.Linq.Expressions.Expression.Parameter(rootType, "root");
            var aNewBody = ParameterExpressionReplacer.Replace(a.Body, memberParam);
            var bNewBody = ParameterExpressionReplacer.Replace(b.Body, memberParam);
            var newBody = System.Linq.Expressions.Expression.And(aNewBody, bNewBody);
            var lambda = System.Linq.Expressions.Expression.Lambda(a.Type, newBody, memberParam);
            return lambda;
        }

        internal static bool IsDerivedFromParameter(this MemberExpression exp)
        {
            ParameterExpression p;
            return IsDerivedFromParameter(exp, out p);
        }
        internal static bool IsDerivedFromParameter(this MemberExpression exp, out ParameterExpression p)
        {
            p = null;

            MemberExpression memberExp = exp;
            System.Linq.Expressions.Expression prevExp;
            do
            {
                prevExp = memberExp.Expression;
                memberExp = prevExp as MemberExpression;
            } while (memberExp != null);

            if (prevExp == null)/* 静态属性访问 */
                return false;

            if (prevExp.NodeType == ExpressionType.Parameter)
            {
                p = (ParameterExpression)prevExp;
                return true;
            }

            /* 当实体继承于某个接口或类时，会有这种情况 */
            if (prevExp.NodeType == ExpressionType.Convert)
            {
                prevExp = ((UnaryExpression)prevExp).Operand;
                if (prevExp.NodeType == ExpressionType.Parameter)
                {
                    p = (ParameterExpression)prevExp;
                    return true;
                }
            }

            return false;
        }

        public static System.Linq.Expressions.Expression StripQuotes(this System.Linq.Expressions.Expression exp)
        {
            while (exp.NodeType == ExpressionType.Quote)
            {
                exp = ((UnaryExpression)exp).Operand;
            }
            return exp;
        }

        public static System.Linq.Expressions.Expression StripConvert(this System.Linq.Expressions.Expression exp)
        {
            System.Linq.Expressions.Expression operand = exp;
            while (operand.NodeType == ExpressionType.Convert || operand.NodeType == ExpressionType.ConvertChecked)
            {
                operand = ((UnaryExpression)operand).Operand;
            }
            return operand;
        }

        public static Stack<MemberExpression> Reverse(this MemberExpression exp)
        {
            var stack = new Stack<MemberExpression>();
            do
            {
                stack.Push(exp);
                exp = exp.Expression as MemberExpression;
            } while (exp != null);

            return stack;
        }

        public static System.Linq.Expressions.Expression MakeWrapperAccess(object value, Type targetType)
        {
            if (value == null)
            {
                if (targetType != null)
                    return System.Linq.Expressions.Expression.Constant(value, targetType);
                else
                    return System.Linq.Expressions.Expression.Constant(value, typeof(object));
            }

            object wrapper = WrapValue(value);
            ConstantExpression wrapperConstantExp = System.Linq.Expressions.Expression.Constant(wrapper);
            System.Linq.Expressions.Expression ret = System.Linq.Expressions.Expression.MakeMemberAccess(wrapperConstantExp, wrapper.GetType().GetProperty("Value"));

            if (ret.Type != targetType)
            {
                ret = System.Linq.Expressions.Expression.Convert(ret, targetType);
            }

            return ret;
        }

        static object WrapValue(object value)
        {
            Type valueType = value.GetType();

            if (valueType == PublicConstants.TypeOfString)
            {
                return new ConstantWrapper<string>((string)value);
            }
            else if (valueType == PublicConstants.TypeOfInt32)
            {
                return new ConstantWrapper<int>((int)value);
            }
            else if (valueType == PublicConstants.TypeOfInt64)
            {
                return new ConstantWrapper<long>((long)value);
            }
            else if (valueType == PublicConstants.TypeOfGuid)
            {
                return new ConstantWrapper<Guid>((Guid)value);
            }

            Type wrapperType = typeof(ConstantWrapper<>).MakeGenericType(valueType);
            ConstructorInfo constructor = wrapperType.GetConstructor(new Type[] { valueType });
            return constructor.Invoke(new object[] { value });
        }
    }
}
