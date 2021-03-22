using System.Data;
using System.Reflection;
using Dapper.Extensions.Expression.DbExpressions;

namespace Dapper.Extensions.Expression.Entity
{
    public class PrimitiveProperty : PropertyBase
    {
        public PrimitiveProperty(PropertyInfo property) : base(property)
        {
            this.ColumnName = property.Name;
        }

        public string ColumnName { get; set; }
        public string SequenceName { get; set; }
        public string SequenceSchema { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsAutoIncrement { get; set; }
        public bool IsNullable { get; set; }
        public bool IsRowVersion { get; set; }
        public DbType DbType { get; set; }
        public int? Size { get; set; }
        public byte? Scale { get; set; }
        public byte? Precision { get; set; }

        public PrimitivePropertyDefinition MakeDefinition()
        {
            PrimitivePropertyDefinition definition = new PrimitivePropertyDefinition(this.Property, new DbColumn(this.ColumnName, this.Property.PropertyType, this.DbType, this.Size, this.Scale, this.Precision), this.IsPrimaryKey, this.IsAutoIncrement, this.IsNullable, this.IsRowVersion, this.SequenceName, this.SequenceSchema, this.Annotations);
            return definition;
        }
    }
}
