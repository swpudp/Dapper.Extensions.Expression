using System;
using System.Linq.Expressions;

namespace Dapper.Extensions.Expression.Entity
{
    public class ComplexPropertyBuilder<TProperty, TEntity> : IComplexPropertyBuilder<TProperty, TEntity>
    {
        public ComplexPropertyBuilder(ComplexProperty property)
        {
            this.Property = property;
        }
        public ComplexProperty Property { get; private set; }

        public IComplexPropertyBuilder<TProperty, TEntity> WithForeignKey(string foreignKey)
        {
            this.Property.ForeignKey = foreignKey;
            return this;
        }
        public IComplexPropertyBuilder<TProperty, TEntity> WithForeignKey<TForeignKey>(Expression<Func<TEntity, TForeignKey>> foreignKey)
        {
            string propertyName = PropertyNameExtractor.Extract(foreignKey);
            this.WithForeignKey(propertyName);
            return this;
        }

        IComplexPropertyBuilder IComplexPropertyBuilder.WithForeignKey(string foreignKey)
        {
            this.Property.ForeignKey = foreignKey;
            return this;
        }
    }
}
