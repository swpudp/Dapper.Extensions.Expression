using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Dapper.Extensions.Expression.Infrastructure;
using Dapper.Extensions.Expression.Reflection;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Extensions.Expression.Mapper
{
    public class ObjectMemberMapper
    {
        Dictionary<MemberInfo, MRMTuple> _mappingMemberMappers;
        Dictionary<MemberInfo, Lazy<MemberValueSetter>> _memberSetters;

        ObjectMemberMapper(Type t)
        {
            this.Type = t;
            this.Init();
        }

        void Init()
        {
            Type t = this.Type;
            var members = t.GetMembers(BindingFlags.Public | BindingFlags.Instance);
            Dictionary<MemberInfo, MRMTuple> mappingMemberMappers = new Dictionary<MemberInfo, MRMTuple>();
            Dictionary<MemberInfo, Lazy<MemberValueSetter>> memberSetters = new Dictionary<MemberInfo, Lazy<MemberValueSetter>>();

            foreach (var member in members)
            {
                if (!member.HasPublicSetter())
                {
                    continue;
                }

                //只支持公共属性和字段
                Type memberType = member.GetMemberType();

                memberSetters.Add(member, new Lazy<MemberValueSetter>(() =>
                {
                    MemberValueSetter valueSetter = MemberValueSetterContainer.GetMemberValueSetter(member);
                    return valueSetter;
                }, LazyThreadSafetyMode.ExecutionAndPublication));

                Infrastructure.MappingType mappingType;
                if (MappingTypeSystem.IsMappingType(memberType, out mappingType))
                {
                    MRMTuple mrmTuple = MRMHelper.CreateMRMTuple(member, mappingType);
                    mappingMemberMappers.Add(member, mrmTuple);
                }
            }

            this._mappingMemberMappers = PublicHelper.Clone(mappingMemberMappers);
            this._memberSetters = PublicHelper.Clone(memberSetters);
        }

        public Type Type { get; private set; }

        public MRMTuple GetMappingMemberMapper(MemberInfo memberInfo)
        {
            memberInfo = memberInfo.AsReflectedMemberOf(this.Type);
            MRMTuple mapperTuple = null;
            this._mappingMemberMappers.TryGetValue(memberInfo, out mapperTuple);
            return mapperTuple;
        }
        public MemberValueSetter GetMemberSetter(MemberInfo memberInfo)
        {
            memberInfo = memberInfo.AsReflectedMemberOf(this.Type);
            Lazy<MemberValueSetter> valueSetter = null;
            this._memberSetters.TryGetValue(memberInfo, out valueSetter);
            return valueSetter.Value;
        }

        static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, ObjectMemberMapper> InstanceCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, ObjectMemberMapper>();

        public static ObjectMemberMapper GetInstance(Type type)
        {
            ObjectMemberMapper instance;
            if (!InstanceCache.TryGetValue(type, out instance))
            {
                lock (type)
                {
                    if (!InstanceCache.TryGetValue(type, out instance))
                    {
                        instance = new ObjectMemberMapper(type);
                        InstanceCache.GetOrAdd(type, instance);
                    }
                }
            }

            return instance;
        }
    }
}
