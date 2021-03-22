using System.Collections.Generic;
using System.Reflection;
using Dapper.Extensions.Expression.Core;
using Dapper.Extensions.Expression.DbExpressions;
using Dapper.Extensions.Expression.Utility;

namespace Dapper.Extensions.Expression.Entity
{
    public class PrimitivePropertyDefinition : PropertyDefinition
    {
        public PrimitivePropertyDefinition(PropertyInfo property, DbColumn column, bool isPrimaryKey, bool isAutoIncrement, bool isNullable, bool isRowVersion, string sequenceName, string sequenceSchema, IList<object> annotations) : base(property, annotations)
        {
            PublicHelper.CheckNull(column, nameof(column));

            this.Column = column;
            this.IsPrimaryKey = isPrimaryKey;
            this.IsAutoIncrement = isAutoIncrement;
            this.IsNullable = isNullable;
            this.IsRowVersion = isRowVersion;
            this.SequenceName = sequenceName;
            this.SequenceSchema = sequenceSchema;
        }
        public override TypeKind Kind { get { return TypeKind.Primitive; } }
        public DbColumn Column { get; private set; }
        public bool IsPrimaryKey { get; private set; }
        public bool IsAutoIncrement { get; private set; }
        public bool IsNullable { get; private set; }
        public bool IsRowVersion { get; private set; }
        public string SequenceName { get; private set; }
        public string SequenceSchema { get; private set; }
    }
}
