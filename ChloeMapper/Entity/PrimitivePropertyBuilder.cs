using System;
using System.Data;

namespace Dapper.Extensions.Expression.Entity
{
    public class PrimitivePropertyBuilder<TProperty> : IPrimitivePropertyBuilder<TProperty>
    {
        public PrimitivePropertyBuilder(PrimitiveProperty property)
        {
            this.Property = property;
        }
        public PrimitiveProperty Property { get; private set; }

        IPrimitivePropertyBuilder AsNonGenericBuilder()
        {
            return this;
        }

        public IPrimitivePropertyBuilder<TProperty> MapTo(string column)
        {
            this.AsNonGenericBuilder().MapTo(column);
            return this;
        }
        IPrimitivePropertyBuilder IPrimitivePropertyBuilder.MapTo(string column)
        {
            this.Property.ColumnName = column;
            return this;
        }

        public IPrimitivePropertyBuilder<TProperty> HasAnnotation(object value)
        {
            this.AsNonGenericBuilder().HasAnnotation(value);
            return this;
        }
        IPrimitivePropertyBuilder IPrimitivePropertyBuilder.HasAnnotation(object value)
        {
            if (value == null)
                throw new ArgumentNullException();

            this.Property.Annotations.Add(value);
            return this;
        }

        public IPrimitivePropertyBuilder<TProperty> IsPrimaryKey(bool isPrimaryKey = true)
        {
            this.AsNonGenericBuilder().IsPrimaryKey(isPrimaryKey);
            return this;
        }
        IPrimitivePropertyBuilder IPrimitivePropertyBuilder.IsPrimaryKey(bool isPrimaryKey)
        {
            this.Property.IsPrimaryKey = isPrimaryKey;
            return this;
        }

        public IPrimitivePropertyBuilder<TProperty> IsAutoIncrement(bool isAutoIncrement = true)
        {
            this.AsNonGenericBuilder().IsAutoIncrement(isAutoIncrement);
            return this;
        }
        IPrimitivePropertyBuilder IPrimitivePropertyBuilder.IsAutoIncrement(bool isAutoIncrement)
        {
            this.Property.IsAutoIncrement = isAutoIncrement;
            if (isAutoIncrement)
            {
                this.Property.SequenceName = null;
                this.Property.SequenceSchema = null;
            }

            return this;
        }

        public IPrimitivePropertyBuilder<TProperty> IsNullable(bool isNullable = true)
        {
            this.AsNonGenericBuilder().IsNullable(isNullable);
            return this;
        }
        IPrimitivePropertyBuilder IPrimitivePropertyBuilder.IsNullable(bool isNullable)
        {
            this.Property.IsNullable = isNullable;
            return this;
        }

        public IPrimitivePropertyBuilder<TProperty> IsRowVersion(bool isRowVersion = true)
        {
            this.AsNonGenericBuilder().IsRowVersion(isRowVersion);
            return this;
        }
        IPrimitivePropertyBuilder IPrimitivePropertyBuilder.IsRowVersion(bool isRowVersion)
        {
            this.Property.IsRowVersion = isRowVersion;
            return this;
        }

        public IPrimitivePropertyBuilder<TProperty> HasDbType(DbType dbType)
        {
            this.AsNonGenericBuilder().HasDbType(dbType);
            return this;
        }
        IPrimitivePropertyBuilder IPrimitivePropertyBuilder.HasDbType(DbType dbType)
        {
            this.Property.DbType = dbType;
            return this;
        }

        public IPrimitivePropertyBuilder<TProperty> HasSize(int? size)
        {
            this.AsNonGenericBuilder().HasSize(size);
            return this;
        }
        IPrimitivePropertyBuilder IPrimitivePropertyBuilder.HasSize(int? size)
        {
            this.Property.Size = size;
            return this;
        }

        public IPrimitivePropertyBuilder<TProperty> HasScale(byte? scale)
        {
            this.AsNonGenericBuilder().HasScale(scale);
            return this;
        }
        IPrimitivePropertyBuilder IPrimitivePropertyBuilder.HasScale(byte? scale)
        {
            this.Property.Scale = scale;
            return this;
        }

        public IPrimitivePropertyBuilder<TProperty> HasPrecision(byte? precision)
        {
            this.AsNonGenericBuilder().HasPrecision(precision);
            return this;
        }
        IPrimitivePropertyBuilder IPrimitivePropertyBuilder.HasPrecision(byte? precision)
        {
            this.Property.Precision = precision;
            return this;
        }

        public IPrimitivePropertyBuilder<TProperty> HasSequence(string name, string schema)
        {
            this.AsNonGenericBuilder().HasSequence(name, schema);
            return this;
        }
        IPrimitivePropertyBuilder IPrimitivePropertyBuilder.HasSequence(string name, string schema)
        {
            this.Property.SequenceName = name;
            this.Property.SequenceSchema = schema;
            if (!string.IsNullOrEmpty(name))
            {
                this.Property.IsAutoIncrement = false;
            }

            return this;
        }
    }
}
