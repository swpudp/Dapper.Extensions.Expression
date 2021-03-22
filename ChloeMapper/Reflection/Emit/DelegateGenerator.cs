using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using Dapper.Extensions.Expression.Data;
using Dapper.Extensions.Expression.Extensions;
using Dapper.Extensions.Expression.Mapper;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Extensions.Expression.Reflection.Emit
{
    public static class DelegateGenerator
    {
        public static Func<IDataReader, int, object> CreateDataReaderGetValueHandler(Type valueType)
        {
            var reader = System.Linq.Expressions.Expression.Parameter(typeof(IDataReader), "reader");
            var ordinal = System.Linq.Expressions.Expression.Parameter(typeof(int), "ordinal");

            var readerMethod = DataReaderConstant.GetReaderMethod(valueType);

            var getValue = System.Linq.Expressions.Expression.Call(null, readerMethod, reader, ordinal);
            var toObject = System.Linq.Expressions.Expression.Convert(getValue, typeof(object));

            var lambda = System.Linq.Expressions.Expression.Lambda<Func<IDataReader, int, object>>(toObject, reader, ordinal);
            var del = lambda.Compile();

            return del;
        }

        public static Action<object, IDataReader, int> CreateSetValueFromReaderDelegate(MemberInfo member)
        {
            var p = System.Linq.Expressions.Expression.Parameter(typeof(object), "instance");
            var instance = System.Linq.Expressions.Expression.Convert(p, member.DeclaringType);
            var reader = System.Linq.Expressions.Expression.Parameter(typeof(IDataReader), "reader");
            var ordinal = System.Linq.Expressions.Expression.Parameter(typeof(int), "ordinal");

            var readerMethod = DataReaderConstant.GetReaderMethod(member.GetMemberType());
            var getValue = System.Linq.Expressions.Expression.Call(null, readerMethod, reader, ordinal);
            var assign = ExpressionExtension.Assign(member, instance, getValue);
            var lambda = System.Linq.Expressions.Expression.Lambda<Action<object, IDataReader, int>>(assign, p, reader, ordinal);

            Action<object, IDataReader, int> del = lambda.Compile();

            return del;
        }

        public static InstanceCreator CreateInstanceCreator(ConstructorInfo constructor)
        {
            PublicHelper.CheckNull(constructor);

            var pExp_reader = System.Linq.Expressions.Expression.Parameter(typeof(IDataReader), "reader");
            var pExp_argumentActivators = System.Linq.Expressions.Expression.Parameter(typeof(List<IObjectActivator>), "argumentActivators");
            var getItemMethod = typeof(List<IObjectActivator>).GetMethod("get_Item");

            ParameterInfo[] parameters = constructor.GetParameters();
            List<System.Linq.Expressions.Expression> arguments = new List<System.Linq.Expressions.Expression>(parameters.Length);

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];

                //IObjectActivator oa = argumentActivators[i];
                var oa = System.Linq.Expressions.Expression.Call(pExp_argumentActivators, getItemMethod, System.Linq.Expressions.Expression.Constant(i));
                //object obj = oa.CreateInstance(IDataReader reader);
                var obj = System.Linq.Expressions.Expression.Call(oa, typeof(IObjectActivator).GetMethod("CreateInstance"), pExp_reader);
                //T argument = (T)obj;
                var argument = System.Linq.Expressions.Expression.Convert(obj, parameter.ParameterType);
                arguments.Add(argument);
            }

            var body = System.Linq.Expressions.Expression.New(constructor, arguments);
            InstanceCreator ret = System.Linq.Expressions.Expression.Lambda<InstanceCreator>(body, pExp_reader, pExp_argumentActivators).Compile();

            return ret;
        }

        public static MemberValueSetter CreateValueSetter(MemberInfo propertyOrField)
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
        public static MemberValueGetter CreateValueGetter(MemberInfo propertyOrField)
        {
            ParameterExpression p = System.Linq.Expressions.Expression.Parameter(typeof(object), "a");
            System.Linq.Expressions.Expression instance = null;
            if (!propertyOrField.IsStaticMember())
            {
                instance = System.Linq.Expressions.Expression.Convert(p, propertyOrField.DeclaringType);
            }

            var memberAccess = System.Linq.Expressions.Expression.MakeMemberAccess(instance, propertyOrField);

            Type type = ReflectionExtension.GetMemberType(propertyOrField);

            System.Linq.Expressions.Expression body = memberAccess;
            if (type.IsValueType)
            {
                body = System.Linq.Expressions.Expression.Convert(memberAccess, typeof(object));
            }

            var lambda = System.Linq.Expressions.Expression.Lambda<MemberValueGetter>(body, p);
            MemberValueGetter ret = lambda.Compile();

            return ret;
        }

        public static Func<object> CreateInstanceActivator(Type type)
        {
            var body = System.Linq.Expressions.Expression.New(type.GetDefaultConstructor());
            var ret = System.Linq.Expressions.Expression.Lambda<Func<object>>(body).Compile();
            return ret;
        }

        public static MethodInvoker CreateMethodInvoker(MethodInfo method)
        {
            List<ParameterExpression> parameterExps = new List<ParameterExpression>();
            ParameterExpression p = System.Linq.Expressions.Expression.Parameter(typeof(object), "instance");
            parameterExps.Add(p);

            ParameterExpression pParameterArray = System.Linq.Expressions.Expression.Parameter(typeof(object[]), "parameters");
            parameterExps.Add(pParameterArray);

            System.Linq.Expressions.Expression instance = null;
            if (!method.IsStatic)
            {
                instance = System.Linq.Expressions.Expression.Convert(p, method.ReflectedType);
            }

            ParameterInfo[] parameters = method.GetParameters();
            List<System.Linq.Expressions.Expression> argumentExps = new List<System.Linq.Expressions.Expression>(parameters.Length);

            var getItemMethod = typeof(object[]).GetMethod("GetValue", new Type[] { typeof(int) });

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];

                //object parameter = parameters[i];
                var parameterExp = System.Linq.Expressions.Expression.Call(pParameterArray, getItemMethod, System.Linq.Expressions.Expression.Constant(i));
                //T argument = (T)parameter;
                var argumentExp = System.Linq.Expressions.Expression.Convert(parameterExp, parameter.ParameterType);
                argumentExps.Add(argumentExp);
            }

            //instance.Method(parameters)
            MethodCallExpression methodCallExp = System.Linq.Expressions.Expression.Call(instance, method, argumentExps);

            MethodInvoker ret;
            if (method.ReturnType == typeof(void))
            {
                var act = System.Linq.Expressions.Expression.Lambda<Action<object, object[]>>(methodCallExp, parameterExps).Compile();
                ret = MakeMethodInvoker(act);
            }
            else
            {
                ret = System.Linq.Expressions.Expression.Lambda<MethodInvoker>(System.Linq.Expressions.Expression.Convert(methodCallExp, typeof(object)), parameterExps).Compile();
                ret = MakeMethodInvoker(ret);
            }

            return ret;
        }
        static MethodInvoker MakeMethodInvoker(Action<object, object[]> act)
        {
            MethodInvoker ret = (object instance, object[] parameters) =>
            {
                act(instance, parameters ?? ReflectionExtension.EmptyArray);
                return null;
            };

            return ret;
        }
        static MethodInvoker MakeMethodInvoker(MethodInvoker methodInvoker)
        {
            MethodInvoker ret = (object instance, object[] parameters) =>
            {
                object val = methodInvoker(instance, parameters ?? ReflectionExtension.EmptyArray);
                return val;
            };

            return ret;
        }
    }
}
