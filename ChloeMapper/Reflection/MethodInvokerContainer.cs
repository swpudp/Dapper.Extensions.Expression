using System.Reflection;
using Dapper.Extensions.Expression.Reflection.Emit;

namespace Dapper.Extensions.Expression.Reflection
{
    public class MethodInvokerContainer
    {
        static readonly System.Collections.Concurrent.ConcurrentDictionary<MethodInfo, MethodInvoker> Cache = new System.Collections.Concurrent.ConcurrentDictionary<MethodInfo, MethodInvoker>();
        public static MethodInvoker GetMethodInvoker(MethodInfo method)
        {
            MethodInvoker invoker = null;
            if (!Cache.TryGetValue(method, out invoker))
            {
                lock (method)
                {
                    if (!Cache.TryGetValue(method, out invoker))
                    {
                        invoker = DelegateGenerator.CreateMethodInvoker(method);
                        Cache.GetOrAdd(method, invoker);
                    }
                }
            }

            return invoker;
        }
    }
}
