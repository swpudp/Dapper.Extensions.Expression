using System.Reflection;
using Dapper.Extensions.Expression.Reflection.Emit;

namespace Dapper.Extensions.Expression.Reflection
{
    public class MemberValueGetterContainer
    {
        static readonly System.Collections.Concurrent.ConcurrentDictionary<MemberInfo, MemberValueGetter> Cache = new System.Collections.Concurrent.ConcurrentDictionary<MemberInfo, MemberValueGetter>();
        public static MemberValueGetter GetMemberValueGetter(MemberInfo memberInfo)
        {
            MemberValueGetter getter = null;
            if (!Cache.TryGetValue(memberInfo, out getter))
            {
                lock (memberInfo)
                {
                    if (!Cache.TryGetValue(memberInfo, out getter))
                    {
                        getter = DelegateGenerator.CreateValueGetter(memberInfo);
                        Cache.GetOrAdd(memberInfo, getter);
                    }
                }
            }

            return getter;
        }
    }
}
