using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dapper.Extensions.Expression.Core;

namespace Dapper.Extensions.Expression.Entity
{
    public class CollectionPropertyDefinition : PropertyDefinition
    {
        public CollectionPropertyDefinition(PropertyInfo property, IList<object> annotations) : base(property, annotations)
        {
            this.ElementType = property.PropertyType.GetGenericArguments().First();
        }
        public override TypeKind Kind { get { return TypeKind.Collection; } }
        public Type ElementType { get; private set; }
    }
}
