using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Dapper.Extensions.Expression.Extensions;

namespace Dapper.Extensions.Expression.Utilities
{
    internal delegate object MethodInvoker(object instance, params object[] parameters);

    internal delegate object MemberValueGetter(object instance);

    internal delegate void MemberValueSetter(object instance, object value);

    internal static class DelegateGenerator
    {
        private static readonly object[] EmptyArray = new object[0];

        internal static MemberValueSetter CreateValueSetter(MemberInfo propertyOrField)
        {
            ParameterExpression p = System.Linq.Expressions.Expression.Parameter(typeof(object), "instance");
            ParameterExpression pValue = System.Linq.Expressions.Expression.Parameter(typeof(object), "value");
            System.Linq.Expressions.Expression instance = null;
            if (!propertyOrField.IsStaticMember())
            {
                instance = System.Linq.Expressions.Expression.Convert(p, propertyOrField.DeclaringType);
            }

            var value = System.Linq.Expressions.Expression.Convert(pValue, propertyOrField.GetMemberType());
            var setValue = ExpressionExtension.Assign(propertyOrField, instance, value);

            System.Linq.Expressions.Expression body = setValue;

            var lambda = System.Linq.Expressions.Expression.Lambda<MemberValueSetter>(body, p, pValue);
            MemberValueSetter ret = lambda.Compile();

            return ret;
        }
        internal static MemberValueGetter CreateValueGetter(MemberInfo propertyOrField)
        {
            ParameterExpression p = System.Linq.Expressions.Expression.Parameter(typeof(object), "a");
            System.Linq.Expressions.Expression instance = null;
            if (!propertyOrField.IsStaticMember())
            {
                instance = System.Linq.Expressions.Expression.Convert(p, propertyOrField.DeclaringType);
            }

            var memberAccess = System.Linq.Expressions.Expression.MakeMemberAccess(instance, propertyOrField);

            Type type = GetMemberType(propertyOrField);

            System.Linq.Expressions.Expression body = memberAccess;
            if (type.IsValueType)
            {
                body = System.Linq.Expressions.Expression.Convert(memberAccess, typeof(object));
            }

            var lambda = System.Linq.Expressions.Expression.Lambda<MemberValueGetter>(body, p);
            MemberValueGetter ret = lambda.Compile();

            return ret;
        }

        internal static MethodInvoker CreateMethodInvoker(MethodInfo method)
        {
            List<ParameterExpression> parameterExpressions = new List<ParameterExpression>();
            ParameterExpression p = System.Linq.Expressions.Expression.Parameter(typeof(object), "instance");
            parameterExpressions.Add(p);

            ParameterExpression pParameterArray = System.Linq.Expressions.Expression.Parameter(typeof(object[]), "parameters");
            parameterExpressions.Add(pParameterArray);

            System.Linq.Expressions.Expression instance = null;
            if (!method.IsStatic)
            {
                instance = System.Linq.Expressions.Expression.Convert(p, method.ReflectedType);
            }

            ParameterInfo[] parameters = method.GetParameters();
            List<System.Linq.Expressions.Expression> argExpressions = new List<System.Linq.Expressions.Expression>(parameters.Length);

            var getItemMethod = typeof(object[]).GetMethod("GetValue", new Type[] { typeof(int) });

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];

                //object parameter = parameters[i];
                var parameterExp = System.Linq.Expressions.Expression.Call(pParameterArray, getItemMethod, System.Linq.Expressions.Expression.Constant(i));
                //T argument = (T)parameter;
                var argumentExp = System.Linq.Expressions.Expression.Convert(parameterExp, parameter.ParameterType);
                argExpressions.Add(argumentExp);
            }

            //instance.Method(parameters)
            MethodCallExpression methodCallExp = System.Linq.Expressions.Expression.Call(instance, method, argExpressions);

            MethodInvoker ret;
            if (method.ReturnType == typeof(void))
            {
                var act = System.Linq.Expressions.Expression.Lambda<Action<object, object[]>>(methodCallExp, parameterExpressions).Compile();
                ret = MakeMethodInvoker(act);
            }
            else
            {
                ret = System.Linq.Expressions.Expression.Lambda<MethodInvoker>(System.Linq.Expressions.Expression.Convert(methodCallExp, typeof(object)), parameterExpressions).Compile();
                ret = MakeMethodInvoker(ret);
            }

            return ret;
        }

        private static MethodInvoker MakeMethodInvoker(Action<object, object[]> act)
        {
            object Ret(object instance, object[] parameters)
            {
                act(instance, parameters ?? EmptyArray);
                return null;
            }
            return Ret;
        }

        private static MethodInvoker MakeMethodInvoker(MethodInvoker methodInvoker)
        {
            object Ret(object instance, object[] parameters)
            {
                object val = methodInvoker(instance, parameters ?? EmptyArray);
                return val;
            }
            return Ret;
        }

        internal static bool IsStaticMember(this MemberInfo propertyOrField)
        {
            if (propertyOrField.MemberType == MemberTypes.Property)
            {
                MethodInfo getter = ((PropertyInfo)propertyOrField).GetMethod;
                return getter.IsStatic;
            }

            if (propertyOrField.MemberType == MemberTypes.Field && propertyOrField is FieldInfo field && field.IsStatic)
                return true;

            return false;
        }

        internal static Type GetMemberType(this MemberInfo propertyOrField)
        {
            if (propertyOrField.MemberType == MemberTypes.Property)
                return ((PropertyInfo)propertyOrField).PropertyType;

            if (propertyOrField.MemberType == MemberTypes.Field)
                return ((FieldInfo)propertyOrField).FieldType;

            throw new ArgumentException();
        }
    }
}
