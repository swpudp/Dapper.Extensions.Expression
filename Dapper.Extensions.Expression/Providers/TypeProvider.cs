using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Dapper.Extensions.Expression.Providers
{
    internal static class TypeProvider
    {
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> TypeTableName = new ConcurrentDictionary<RuntimeTypeHandle, string>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> KeyProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> ExplicitKeyProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> TypeProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> ComputedProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> NotMappedProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> CanWriteProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> CanUpdateProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> CanQueryProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();

        private static List<PropertyInfo> GetComputedProperties(Type type)
        {
            if (ComputedProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> pi))
            {
                return pi.ToList();
            }

            List<PropertyInfo> computedProperties = GetAllProperties(type).Where(p => p.GetCustomAttributes(true).Any(a => a is ComputedAttribute)).ToList();

            ComputedProperties[type.TypeHandle] = computedProperties;
            return computedProperties;
        }

        /// <summary>
        /// 获取可查询属性
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static IList<PropertyInfo> GetCanQueryProperties(Type type)
        {
            if (CanQueryProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> propertyInfos))
            {
                return propertyInfos.ToList();
            }
            List<PropertyInfo> props = GetAllProperties(type);
            IEnumerable<PropertyInfo> notMappedProperties = GetNotMappedProperties(type);
            IList<PropertyInfo> canQueryProperties = props.Except(notMappedProperties).ToList();
            CanQueryProperties[type.TypeHandle] = canQueryProperties;
            return canQueryProperties;
        }

        private static IEnumerable<PropertyInfo> GetNotMappedProperties(Type type)
        {
            if (NotMappedProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> pi))
            {
                return pi.ToList();
            }
            List<PropertyInfo> notMappedPropertyInfos = GetAllProperties(type).Where(p => p.GetCustomAttributes(true).Any(a => a is NotMappedAttribute)).ToList();
            NotMappedProperties[type.TypeHandle] = notMappedPropertyInfos;
            return notMappedPropertyInfos;
        }

        internal static string GetTableName(Type type)
        {
            if (TypeTableName.TryGetValue(type.TypeHandle, out string name))
            {
                return name;
            }
            TableAttribute tableAttr = type.GetCustomAttribute<TableAttribute>(false);
            name = tableAttr != null ? tableAttr.Name : type.Name;
            TypeTableName[type.TypeHandle] = name;
            return name;
        }

        private static List<PropertyInfo> GetAllProperties(Type type)
        {
            if (TypeProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> pis))
            {
                return pis.ToList();
            }
            List<PropertyInfo> properties = type.GetProperties().ToList();
            TypeProperties[type.TypeHandle] = properties;
            return properties.ToList();
        }

        private static IList<PropertyInfo> GetKeyProperties(Type type)
        {
            if (KeyProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> pi))
            {
                return pi.ToList();
            }

            List<PropertyInfo> allProperties = GetAllProperties(type);
            List<PropertyInfo> keyProperties = allProperties.Where(p => p.GetCustomAttributes(true).Any(a => a is KeyAttribute)).ToList();

            if (keyProperties.Count == 0)
            {
                PropertyInfo idProp = allProperties.Find(p => string.Equals(p.Name, "id", StringComparison.CurrentCultureIgnoreCase));
                if (idProp != null && !idProp.GetCustomAttributes(true).Any(a => a is ExplicitKeyAttribute))
                {
                    keyProperties.Add(idProp);
                }
            }
            KeyProperties[type.TypeHandle] = keyProperties;
            return keyProperties;
        }

        /// <summary>
        /// 获取可写入列
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static IList<PropertyInfo> GetCanWriteProperties(Type type)
        {
            if (CanWriteProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> propertyInfos))
            {
                return propertyInfos.ToList();
            }
            List<PropertyInfo> allProperties = GetAllProperties(type);
            IEnumerable<PropertyInfo> keyProperties = GetKeyProperties(type);
            List<PropertyInfo> computedProperties = GetComputedProperties(type);
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
        internal static IList<PropertyInfo> GetCanUpdateProperties(Type type)
        {
            if (CanUpdateProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> properties))
            {
                return properties.ToList();
            }
            IList<PropertyInfo> keyProperties = GetUpdateKeyProperties(type);
            List<PropertyInfo> allProperties = GetAllProperties(type);
            List<PropertyInfo> computedProperties = GetComputedProperties(type);
            IEnumerable<PropertyInfo> notMappedProperties = GetNotMappedProperties(type);
            List<PropertyInfo> nonIdProps = allProperties.Except(keyProperties.Union(computedProperties).Union(notMappedProperties)).ToList();
            CanUpdateProperties[type.TypeHandle] = nonIdProps;
            return nonIdProps;
        }

        internal static IList<PropertyInfo> GetUpdateKeyProperties(Type type)
        {
            List<PropertyInfo> keyProperties = GetKeyProperties(type).ToList();
            List<PropertyInfo> explicitKeyProperties = GetExplicitKeyProperties(type);
            if (keyProperties.Count == 0 && explicitKeyProperties.Count == 0)
            {
                throw new ArgumentException("Entity must have at least one [Key] or [ExplicitKey] property");
            }
            keyProperties.AddRange(explicitKeyProperties);
            return keyProperties;
        }

        private static List<PropertyInfo> GetExplicitKeyProperties(Type type)
        {
            if (ExplicitKeyProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> pi))
            {
                return pi.ToList();
            }
            List<PropertyInfo> explicitKeyProperties = GetAllProperties(type).Where(p => p.GetCustomAttributes(true).Any(a => a is ExplicitKeyAttribute)).ToList();
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
            if (type.IsGenericType)
            {
                TypeInfo typeInfo = type.GetTypeInfo();
                bool isEnumerable =
                    typeInfo.ImplementedInterfaces.Any(ti => ti.IsGenericType && ti.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                    typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>);
                if (isEnumerable)
                {
                    eleType = type.GetGenericArguments()[0];
                    return true;
                }
            }
            return false;
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

        internal static PropertyInfo GetSingleKey<T>(string method)
        {
            var type = typeof(T);
            var keys = GetKeyProperties(type);
            var explicitKeys = GetExplicitKeyProperties(type);
            var keyCount = keys.Count + explicitKeys.Count;
            if (keyCount > 1)
                throw new DataException($"{method}<T> only supports an entity with a single [Key] or [ExplicitKey] property. [Key] Count: {keys.Count}, [ExplicitKey] Count: {explicitKeys.Count}");
            if (keyCount == 0)
                throw new DataException($"{method}<T> only supports an entity with a [Key] or an [ExplicitKey] property");

            return keys.Count > 0 ? keys[0] : explicitKeys[0];
        }

        internal static readonly IList<Type> AllowTypes = new List<Type>
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
    }
}
