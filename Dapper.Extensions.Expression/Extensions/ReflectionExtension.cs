﻿using Dapper.Extensions.Expression.Utilities;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Dapper.Extensions.Expression.Extensions
{
    internal static class ReflectionExtension
    {
        /// <summary>
        /// 成员方法执行
        /// </summary>
        private static readonly ConcurrentDictionary<MemberInfo, MethodInvoker> MethodInvokerCache = new ConcurrentDictionary<MemberInfo, MethodInvoker>();

        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="method"></param>
        /// <param name="obj"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        internal static object Invoke(this MethodInfo method, object obj, params object[] parameters)
        {
            if (MethodInvokerCache.TryGetValue(method, out MethodInvoker invoker))
            {
                return invoker(obj, parameters);
            }
            invoker = DelegateGenerator.CreateMethodInvoker(method);
            MethodInvokerCache.GetOrAdd(method, invoker);
            return invoker(obj, parameters);
        }


        private static readonly ConcurrentDictionary<MemberInfo, MemberValueGetter> Cache = new ConcurrentDictionary<MemberInfo, MemberValueGetter>();

        internal static object GetValue(this MemberInfo memberInfo, object obj)
        {
            if (Cache.TryGetValue(memberInfo, out MemberValueGetter getter))
            {
                return getter(obj);
            }
            getter = DelegateGenerator.CreateValueGetter(memberInfo);
            Cache.GetOrAdd(memberInfo, getter);
            return getter(obj);
        }

        internal static bool IsNotMapped(this MemberInfo member)
        {
            return member.IsDefined(typeof(NotMappedAttribute), true);
        }

        internal static bool IsColumnAlias(this MemberInfo memberInfo)
        {
            return memberInfo.IsDefined(typeof(ColumnAttribute), true);
        }

        internal static bool IsNullable(this Type type)
        {
            return IsNullable(type, out _);
        }
        internal static bool IsNullable(this Type type, out Type underlyingType)
        {
            underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType != null;
        }
        internal static Type GetUnderlyingType(this Type type)
        {
            if (!IsNullable(type, out Type underlyingType))
            {
                underlyingType = type;
            }
            return underlyingType;
        }

        internal static void SetMemberValue(this MemberInfo propertyOrField, object obj, object value)
        {
            if (propertyOrField.MemberType == MemberTypes.Property)
            {
                ((PropertyInfo)propertyOrField).SetValue(obj, value, null);
                return;
            }
            if (propertyOrField.MemberType == MemberTypes.Field)
            {
                ((FieldInfo)propertyOrField).SetValue(obj, value);
                return;
            }
            throw new ArgumentException();
        }
    }
}