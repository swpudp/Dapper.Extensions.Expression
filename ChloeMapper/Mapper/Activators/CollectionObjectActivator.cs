using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Dapper.Extensions.Expression.Reflection.Emit;

namespace Dapper.Extensions.Expression.Mapper.Activators
{
    public class CollectionObjectActivator : ObjectActivatorBase, IObjectActivator
    {
        Type _collectionType;
        Func<object> _activator;

        static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, Func<object>> ActivatorCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, Func<object>>();

        static Func<object> GetActivator(Type collectionType)
        {
            Func<object> activator = ActivatorCache.GetOrAdd(collectionType, type =>
           {
               var typeDefinition = type.GetGenericTypeDefinition();
               Type implTypeDefinition = null;
               if (typeDefinition.IsAssignableFrom(typeof(List<>)))
               {
                   implTypeDefinition = typeof(List<>);
               }
               else if (typeDefinition.IsAssignableFrom(typeof(Collection<>)))
               {
                   implTypeDefinition = typeof(Collection<>);
               }
               else
               {
                   throw new NotSupportedException($"Not supported collection type '{type.Name}'");
               }

               return DelegateGenerator.CreateInstanceActivator(implTypeDefinition.MakeGenericType(type.GetGenericArguments()[0]));
           });

            return activator;
        }

        public CollectionObjectActivator(Type collectionType)
        {
            this._collectionType = collectionType;
            this._activator = GetActivator(collectionType);
        }

        public override object CreateInstance(IDataReader reader)
        {
            return this._activator();
        }
    }
}
