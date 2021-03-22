using System;
using Dapper.Extensions.Expression.Infrastructure;

namespace Dapper.Extensions.Expression.Descriptors
{
    public static class PropertyDescriptorExtension
    {
        public static TypeDescriptor GetPropertyTypeDescriptor(this PropertyDescriptor propertyDescriptor)
        {
            ComplexPropertyDescriptor complexPropertyDescriptor = propertyDescriptor as ComplexPropertyDescriptor;

            Type type;
            if (complexPropertyDescriptor != null)
            {
                type = propertyDescriptor.PropertyType;
            }
            else
            {
                type = (propertyDescriptor as CollectionPropertyDescriptor).ElementType;
            }

            return EntityTypeContainer.GetDescriptor(type);
        }
    }
}
