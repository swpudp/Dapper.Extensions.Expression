using System.Reflection;
using Dapper.Extensions.Expression.Reflection.Emit;

namespace Dapper.Extensions.Expression.Reflection
{
    public class MemberValueSetterContainer
    {
        static readonly System.Collections.Concurrent.ConcurrentDictionary<MemberInfo, MemberValueSetter> Cache = new System.Collections.Concurrent.ConcurrentDictionary<MemberInfo, MemberValueSetter>();
        public static MemberValueSetter GetMemberValueSetter(MemberInfo memberInfo)
        {
            MemberValueSetter setter = null;
            if (!Cache.TryGetValue(memberInfo, out setter))
            {
                lock (memberInfo)
                {
                    if (!Cache.TryGetValue(memberInfo, out setter))
                    {
                        setter = DelegateGenerator.CreateValueSetter(memberInfo);
                        Cache.GetOrAdd(memberInfo, setter);
                    }
                }
            }

            return setter;
        }
    }
}
