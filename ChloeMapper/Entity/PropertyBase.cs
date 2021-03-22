using System.Collections.Generic;
using System.Reflection;

namespace Dapper.Extensions.Expression.Entity
{
    public abstract class PropertyBase
    {
        protected PropertyBase(PropertyInfo property)
        {
            this.Property = property;
        }
        public PropertyInfo Property { get; private set; }
        public List<object> Annotations { get; private set; } = new List<object>();
    }
}
