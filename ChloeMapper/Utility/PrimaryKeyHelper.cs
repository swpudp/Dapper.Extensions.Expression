using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dapper.Extensions.Expression.Descriptors;
using Dapper.Extensions.Expression.Extensions;
using Dapper.Extensions.Expression.Infrastructure;
using Dapper.Extensions.Expression.Reflection;

namespace Dapper.Extensions.Expression.Utility
{
    public static class PrimaryKeyHelper
    {
        public static void KeyValueNotNull(PrimitivePropertyDescriptor keyPropertyDescriptor, object keyValue)
        {
            if (keyValue == null)
                throw new ArgumentException(string.Format("The primary key '{0}' can not be null.", keyPropertyDescriptor.Property.Name));
        }
        public static Expression<Func<TEntity, bool>> BuildCondition<TEntity>(object key)
        {
            /*
             * key:
             * 如果实体是单一主键，可以传入的 key 与主键属性类型相同的值，亦可以传一个包含了与实体主键类型相同的属性的对象，如：new { Id = 1 }
             * 如果实体是多主键，则传入的 key 须是包含了与实体主键类型相同的属性的对象，如：new { Key1 = "1", Key2 = "2" }
             */

            PublicHelper.CheckNull(key);

            Type entityType = typeof(TEntity);
            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(entityType);
            PublicHelper.EnsureHasPrimaryKey(typeDescriptor);

            ParameterExpression parameter = System.Linq.Expressions.Expression.Parameter(entityType, "a");
            System.Linq.Expressions.Expression conditionBody = null;

            Type keyType = key.GetType();
            if (typeDescriptor.PrimaryKeys.Count == 1 && MappingTypeSystem.IsMappingType(keyType))
            {
                /* a => a.Key == key */

                PrimitivePropertyDescriptor keyDescriptor = typeDescriptor.PrimaryKeys[0];
                System.Linq.Expressions.Expression propOrField = System.Linq.Expressions.Expression.PropertyOrField(parameter, keyDescriptor.Property.Name);
                System.Linq.Expressions.Expression wrappedValue = ExpressionExtension.MakeWrapperAccess(key, keyDescriptor.PropertyType);
                conditionBody = System.Linq.Expressions.Expression.Equal(propOrField, wrappedValue);
            }
            else
            {
                /*
                 * key: new { Key1 = "1", Key2 = "2" }
                 */

                /* a => a.Key1 == key.Key1 && a.Key2 == key.Key2 */

                Type keyObjectType = keyType;
                ConstantExpression keyConstantExp = System.Linq.Expressions.Expression.Constant(key);
                if (keyObjectType == entityType)
                {
                    foreach (PrimitivePropertyDescriptor primaryKey in typeDescriptor.PrimaryKeys)
                    {
                        System.Linq.Expressions.Expression propOrField = System.Linq.Expressions.Expression.PropertyOrField(parameter, primaryKey.Property.Name);
                        System.Linq.Expressions.Expression keyValue = System.Linq.Expressions.Expression.MakeMemberAccess(keyConstantExp, primaryKey.Property);
                        System.Linq.Expressions.Expression e = System.Linq.Expressions.Expression.Equal(propOrField, keyValue);
                        conditionBody = conditionBody == null ? e : System.Linq.Expressions.Expression.AndAlso(conditionBody, e);
                    }
                }
                else
                {
                    for (int i = 0; i < typeDescriptor.PrimaryKeys.Count; i++)
                    {
                        PrimitivePropertyDescriptor keyPropertyDescriptor = typeDescriptor.PrimaryKeys[i];
                        MemberInfo keyMember = keyPropertyDescriptor.Property;
                        MemberInfo inputKeyMember = keyObjectType.GetMember(keyMember.Name).FirstOrDefault();
                        if (inputKeyMember == null)
                            throw new ArgumentException(string.Format("The input object does not define property for key '{0}'.", keyMember.Name));

                        System.Linq.Expressions.Expression propOrField = System.Linq.Expressions.Expression.PropertyOrField(parameter, keyMember.Name);
                        System.Linq.Expressions.Expression keyValueExp = System.Linq.Expressions.Expression.MakeMemberAccess(keyConstantExp, inputKeyMember);

                        Type keyMemberType = keyMember.GetMemberType();
                        if (inputKeyMember.GetMemberType() != keyMemberType)
                        {
                            keyValueExp = System.Linq.Expressions.Expression.Convert(keyValueExp, keyMemberType);
                        }
                        System.Linq.Expressions.Expression e = System.Linq.Expressions.Expression.Equal(propOrField, keyValueExp);
                        conditionBody = conditionBody == null ? e : System.Linq.Expressions.Expression.AndAlso(conditionBody, e);
                    }
                }
            }

            Expression<Func<TEntity, bool>> condition = System.Linq.Expressions.Expression.Lambda<Func<TEntity, bool>>(conditionBody, parameter);
            return condition;
        }
        public static Dictionary<PrimitivePropertyDescriptor, object> CreateKeyValueMap(TypeDescriptor typeDescriptor)
        {
            Dictionary<PrimitivePropertyDescriptor, object> keyValueMap = new Dictionary<PrimitivePropertyDescriptor, object>(typeDescriptor.PrimaryKeys.Count);
            foreach (PrimitivePropertyDescriptor keyPropertyDescriptor in typeDescriptor.PrimaryKeys)
            {
                keyValueMap.Add(keyPropertyDescriptor, null);
            }

            return keyValueMap;
        }
    }
}
