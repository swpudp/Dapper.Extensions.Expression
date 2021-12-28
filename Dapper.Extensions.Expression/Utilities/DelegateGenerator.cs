using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Dapper.Extensions.Expression.Utilities
{
    internal delegate object MethodInvoker(object instance, params object[] parameters);

    internal delegate object MemberValueGetter(object instance);

    internal static class DelegateGenerator
    {
        private static readonly object[] EmptyArray = new object[0];

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

            var getItemMethod = typeof(object[]).GetMethod("GetValue", new[] { typeof(int) });

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

        private static bool IsStaticMember(this MemberInfo propertyOrField)
        {
            switch (propertyOrField.MemberType)
            {
                case MemberTypes.Property:
                    MethodInfo getter = ((PropertyInfo)propertyOrField).GetMethod;
                    return getter.IsStatic;
                case MemberTypes.Field when propertyOrField is FieldInfo field && field.IsStatic:
                    return true;
                default:
                    return false;
            }
        }

        private static Type GetMemberType(this MemberInfo propertyOrField)
        {
            switch (propertyOrField.MemberType)
            {
                case MemberTypes.Property:
                    return ((PropertyInfo)propertyOrField).PropertyType;
                case MemberTypes.Field:
                    return ((FieldInfo)propertyOrField).FieldType;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
