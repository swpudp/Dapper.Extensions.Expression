using Dapper.Extensions.Expression.MethodCalls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dapper.Extensions.Expression.Providers
{
    internal static class MethodCallProvider
    {
        private static readonly IDictionary<RuntimeMethodHandle, AbstractMethodCallHandler> MethodCallHandlers = new Dictionary<RuntimeMethodHandle, AbstractMethodCallHandler>();

        private static readonly IList<AbstractMethodCallHandler> MethodCallHandlerInstances = new List<AbstractMethodCallHandler>();

        /// <summary>
        /// 初始化实例
        /// </summary>
        private static void Initialize()
        {
            if (MethodCallHandlerInstances.Any())
            {
                return;
            }
            var methodHandlerTypes = Assembly.GetExecutingAssembly().GetTypes().Where(a => a.IsClass && !a.IsAbstract && typeof(AbstractMethodCallHandler).IsAssignableFrom(a));
            foreach (Type methodHandlerType in methodHandlerTypes)
            {
                AbstractMethodCallHandler instance = (AbstractMethodCallHandler)Activator.CreateInstance(methodHandlerType);
                MethodCallHandlerInstances.Add(instance);
            }
        }

        public static AbstractMethodCallHandler GetCallHandler(MethodCallExpression exp)
        {
            if (MethodCallHandlers.TryGetValue(exp.Method.MethodHandle, out AbstractMethodCallHandler handler))
            {
                return handler;
            }
            Initialize();
            foreach (AbstractMethodCallHandler unAssignHandler in MethodCallHandlerInstances.Where(f => f.MethodName == exp.Method.Name))
            {
                if (!unAssignHandler.IsMatch(exp))
                {
                    continue;
                }
                handler = unAssignHandler;
                MethodCallHandlers.Add(exp.Method.MethodHandle, handler);
                break;
            }
            return handler;
        }
    }
}
