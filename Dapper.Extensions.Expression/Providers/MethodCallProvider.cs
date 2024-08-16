using Dapper.Extensions.Expression.MethodCalls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dapper.Extensions.Expression.Providers
{
    internal static class MethodCallProvider
    {
        private static readonly ConcurrentDictionary<RuntimeMethodHandle, AbstractMethodCallHandler> MethodCallHandlers = new ConcurrentDictionary<RuntimeMethodHandle, AbstractMethodCallHandler>();

        private static readonly IReadOnlyCollection<AbstractMethodCallHandler> MethodCallHandlerInstances;

        /// <summary>
        /// 初始化实例
        /// </summary>
        static MethodCallProvider()
        {
            MethodCallHandlerInstances = Assembly.GetExecutingAssembly().GetTypes()
                .Where(a => a.IsClass && !a.IsAbstract && typeof(AbstractMethodCallHandler).IsAssignableFrom(a))
                .Select(x => (AbstractMethodCallHandler)Activator.CreateInstance(x))
                .ToList();
        }

        public static AbstractMethodCallHandler GetCallHandler(MethodCallExpression exp)
        {
            if (MethodCallHandlers.TryGetValue(exp.Method.MethodHandle, out AbstractMethodCallHandler handler))
            {
                return handler;
            }
            handler = MethodCallHandlerInstances.Where(f => f.MethodName == exp.Method.Name).FirstOrDefault(f => f.IsMatch(exp));
            MethodCallHandlers[exp.Method.MethodHandle] = handler ?? throw new NotSupportedException(exp.Method.Name);
            return handler;
        }
    }
}
