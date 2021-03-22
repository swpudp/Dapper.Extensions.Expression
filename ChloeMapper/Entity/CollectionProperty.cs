using System.Reflection;

namespace Dapper.Extensions.Expression.Entity
{
    public class CollectionProperty : PropertyBase
    {
        public CollectionProperty(PropertyInfo property) : base(property)
        {
        }

        public CollectionPropertyDefinition MakeDefinition()
        {
            CollectionPropertyDefinition definition = new CollectionPropertyDefinition(this.Property, this.Annotations);
            return definition;
        }
    }
}
