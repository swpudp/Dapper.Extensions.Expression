using Dapper.Extensions.Expression.Extensions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dapper.Extensions.Expression.Providers
{
    internal static class TypeProvider
    {
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, List<PropertyInfo>> AutoIncrementProperties = new ConcurrentDictionary<RuntimeTypeHandle, List<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, List<PropertyInfo>> ExplicitKeyProperties = new ConcurrentDictionary<RuntimeTypeHandle, List<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, List<PropertyInfo>> TypeProperties = new ConcurrentDictionary<RuntimeTypeHandle, List<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, List<PropertyInfo>> ComputedProperties = new ConcurrentDictionary<RuntimeTypeHandle, List<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, List<PropertyInfo>> NotMappedProperties = new ConcurrentDictionary<RuntimeTypeHandle, List<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, List<PropertyInfo>> CanWriteProperties = new ConcurrentDictionary<RuntimeTypeHandle, List<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, List<PropertyInfo>> CanUpdateProperties = new ConcurrentDictionary<RuntimeTypeHandle, List<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, List<PropertyInfo>> CanQueryProperties = new ConcurrentDictionary<RuntimeTypeHandle, List<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, ConstantExpression> ConstantTrueExpressions = new ConcurrentDictionary<RuntimeTypeHandle, ConstantExpression>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, ConstantExpression> ConstantFalseExpressions = new ConcurrentDictionary<RuntimeTypeHandle, ConstantExpression>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, ConstantExpression> ConstantNullExpressions = new ConcurrentDictionary<RuntimeTypeHandle, ConstantExpression>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, List<PropertyInfo>> ColumnProperties = new ConcurrentDictionary<RuntimeTypeHandle, List<PropertyInfo>>();

        private static IEnumerable<PropertyInfo> GetComputedProperties(Type type)
        {
            if (ComputedProperties.TryGetValue(type.TypeHandle, out List<PropertyInfo> pi))
            {
                return pi;
            }
            List<PropertyInfo> computedProperties = GetAllProperties(type).Where(p => p.GetCustomAttributes(true).Any(a => a is ComputedAttribute)).ToList();
            ComputedProperties[type.TypeHandle] = computedProperties;
            return computedProperties;
        }

        public static MemberInfo GetColumnProperty(Type type, MemberInfo member)
        {
            if (ColumnProperties.TryGetValue(type.TypeHandle, out List<PropertyInfo> propertyInfos))
            {
                return propertyInfos.FirstOrDefault(f => f.Name == member.Name) ?? member;
            }
            propertyInfos = GetAllProperties(type).Where(f => f.IsColumnAlias()).ToList();
            ColumnProperties[type.TypeHandle] = propertyInfos;
            return propertyInfos.FirstOrDefault(f => f.Name == member.Name) ?? member;
        }

        /// <summary>
        /// 获取可查询属性
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static List<PropertyInfo> GetCanQueryProperties(Type type)
        {
            if (CanQueryProperties.TryGetValue(type.TypeHandle, out List<PropertyInfo> propertyInfos))
            {
                return propertyInfos;
            }
            List<PropertyInfo> props = GetAllProperties(type);
            IEnumerable<PropertyInfo> notMappedProperties = GetNotMappedProperties(type);
            List<PropertyInfo> canQueryProperties = props.Except(notMappedProperties).ToList();
            CanQueryProperties[type.TypeHandle] = canQueryProperties;
            return canQueryProperties;
        }

        private static IEnumerable<PropertyInfo> GetNotMappedProperties(Type type)
        {
            if (NotMappedProperties.TryGetValue(type.TypeHandle, out List<PropertyInfo> pi))
            {
                return pi;
            }
            List<PropertyInfo> notMappedPropertyInfos = GetAllProperties(type).Where(p => p.GetCustomAttributes(true).Any(a => a is NotMappedAttribute)).ToList();
            NotMappedProperties[type.TypeHandle] = notMappedPropertyInfos;
            return notMappedPropertyInfos;
        }

        private static List<PropertyInfo> GetAllProperties(Type type)
        {
            if (TypeProperties.TryGetValue(type.TypeHandle, out List<PropertyInfo> pis))
            {
                return pis;
            }
            List<PropertyInfo> properties = type.GetProperties().ToList();
            TypeProperties[type.TypeHandle] = properties;
            return properties;
        }

        private static List<PropertyInfo> GetAutoIncrementProperties(Type type)
        {
            if (AutoIncrementProperties.TryGetValue(type.TypeHandle, out List<PropertyInfo> pi))
            {
                return pi;
            }
            List<PropertyInfo> allProperties = GetAllProperties(type);
            List<PropertyInfo> keyProperties = allProperties.Where(p => p.GetCustomAttributes(true).Any(a => (a is KeyAttribute keyAttribute) && keyAttribute.IsAutoIncrement)).ToList();
            AutoIncrementProperties[type.TypeHandle] = keyProperties;
            return keyProperties;
        }

        /// <summary>
        /// 获取可写入列
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static List<PropertyInfo> GetCanWriteProperties(Type type)
        {
            if (CanWriteProperties.TryGetValue(type.TypeHandle, out List<PropertyInfo> propertyInfos))
            {
                return propertyInfos;
            }
            List<PropertyInfo> allProperties = GetAllProperties(type);
            IEnumerable<PropertyInfo> keyProperties = GetAutoIncrementProperties(type);
            IEnumerable<PropertyInfo> computedProperties = GetComputedProperties(type);
            IEnumerable<PropertyInfo> notMappedProperties = GetNotMappedProperties(type);
            List<PropertyInfo> canWriteProperties = allProperties.Except(keyProperties.Union(computedProperties).Union(notMappedProperties)).ToList();
            CanWriteProperties[type.TypeHandle] = canWriteProperties;
            return canWriteProperties;
        }

        /// <summary>
        /// 可更新列
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static List<PropertyInfo> GetCanUpdateProperties(Type type)
        {
            if (CanUpdateProperties.TryGetValue(type.TypeHandle, out List<PropertyInfo> properties))
            {
                return properties;
            }
            List<PropertyInfo> keyProperties = GetKeyProperties(type);
            List<PropertyInfo> allProperties = GetAllProperties(type);
            IEnumerable<PropertyInfo> computedProperties = GetComputedProperties(type);
            IEnumerable<PropertyInfo> notMappedProperties = GetNotMappedProperties(type);
            List<PropertyInfo> nonIdProps = allProperties.Except(keyProperties.Union(computedProperties).Union(notMappedProperties)).ToList();
            CanUpdateProperties[type.TypeHandle] = nonIdProps;
            return nonIdProps;
        }

        internal static List<PropertyInfo> GetKeyProperties(Type type)
        {
            if (ExplicitKeyProperties.TryGetValue(type.TypeHandle, out List<PropertyInfo> pi))
            {
                return pi;
            }
            List<PropertyInfo> explicitKeyProperties = GetAllProperties(type).Where(p => p.GetCustomAttributes(true).Any(a => a is KeyAttribute)).ToList();
            ExplicitKeyProperties[type.TypeHandle] = explicitKeyProperties;
            return explicitKeyProperties;
        }

        internal static bool IsList(this Type type, out Type eleType)
        {
            eleType = null;
            if (type.IsArray)
            {
                eleType = type.GetElementType();
                return true;
            }
            if (type == null)
            {
                throw new InvalidOperationException();
            }

            if (!type.IsGenericType) return false;
            TypeInfo typeInfo = type.GetTypeInfo();
            bool isEnumerable =
                typeInfo.ImplementedInterfaces.Any(ti => ti.IsGenericType && ti.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>);
            if (!isEnumerable) return false;
            eleType = type.GetGenericArguments()[0];
            return true;
        }

        /// <summary>
        /// 是否集合Contains
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        internal static bool IsListContains(this MethodInfo methodInfo)
        {
            Type declaringType = methodInfo.DeclaringType;
            if (declaringType == null)
            {
                return false;
            }
            return typeof(IList).IsAssignableFrom(declaringType) || IsInheritCollection(declaringType);
        }

        /// <summary>
        /// 是否集合
        /// </summary>
        /// <param name="declaringType"></param>
        /// <returns></returns>
        private static bool IsInheritCollection(Type declaringType)
        {
            return declaringType.IsGenericType && typeof(ICollection<>).MakeGenericType(declaringType.GetGenericArguments()).IsAssignableFrom(declaringType);
        }

        /// <summary>
        /// 是否迭代器Contains
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        internal static bool IsEnumerableContains(this MethodInfo methodInfo)
        {
            Type declaringType = methodInfo.DeclaringType;
            if (declaringType == null)
            {
                return false;
            }
            return methodInfo.IsStatic && declaringType == typeof(Enumerable);
        }

        internal static readonly List<Type> AllowTypes = new List<Type>
        {
            typeof(int),
            typeof(int?),
            typeof(long),
            typeof(long?),
            typeof(decimal),
            typeof(decimal?),
            typeof(double),
            typeof(double?),
            typeof(float),
            typeof(float?)
        };

        internal static ConstantExpression GetTrueExpression(Type type)
        {
            if (ConstantTrueExpressions.TryGetValue(type.TypeHandle, out ConstantExpression constantExpression))
            {
                return constantExpression;
            }
            constantExpression = System.Linq.Expressions.Expression.Constant(true, type);
            ConstantTrueExpressions[type.TypeHandle] = constantExpression;
            return constantExpression;
        }
        internal static ConstantExpression GetFalseExpression(Type type)
        {
            if (ConstantFalseExpressions.TryGetValue(type.TypeHandle, out ConstantExpression constantExpression))
            {
                return constantExpression;
            }
            constantExpression = System.Linq.Expressions.Expression.Constant(false, type);
            ConstantFalseExpressions[type.TypeHandle] = constantExpression;
            return constantExpression;
        }

        internal static ConstantExpression GetNullExpression(Type type)
        {
            if (ConstantNullExpressions.TryGetValue(type.TypeHandle, out ConstantExpression constantExpression))
            {
                return constantExpression;
            }
            constantExpression = System.Linq.Expressions.Expression.Constant(null, type);
            ConstantNullExpressions[type.TypeHandle] = constantExpression;
            return constantExpression;
        }
    }
}
