using System.Data;

namespace Dapper.Extensions.Expression.Entity
{
    public interface IPrimitivePropertyBuilder
    {
        PrimitiveProperty Property { get; }
        IPrimitivePropertyBuilder MapTo(string column);
        IPrimitivePropertyBuilder HasAnnotation(object value);
        IPrimitivePropertyBuilder IsPrimaryKey(bool isPrimaryKey = true);
        IPrimitivePropertyBuilder IsAutoIncrement(bool autoIncrement = true);
        IPrimitivePropertyBuilder IsNullable(bool isNullable = true);
        IPrimitivePropertyBuilder IsRowVersion(bool isRowVersion = true);
        IPrimitivePropertyBuilder HasDbType(DbType dbType);
        IPrimitivePropertyBuilder HasSize(int? size);
        IPrimitivePropertyBuilder HasScale(byte? scale);
        IPrimitivePropertyBuilder HasPrecision(byte? precision);
        IPrimitivePropertyBuilder HasSequence(string name, string schema = null);
    }
    public interface IPrimitivePropertyBuilder<TProperty> : IPrimitivePropertyBuilder
    {
        new IPrimitivePropertyBuilder<TProperty> MapTo(string column);
        new IPrimitivePropertyBuilder<TProperty> HasAnnotation(object value);
        new IPrimitivePropertyBuilder<TProperty> IsPrimaryKey(bool isPrimaryKey = true);
        new IPrimitivePropertyBuilder<TProperty> IsAutoIncrement(bool autoIncrement = true);
        new IPrimitivePropertyBuilder<TProperty> IsNullable(bool isNullable = true);
        new IPrimitivePropertyBuilder<TProperty> IsRowVersion(bool isRowVersion = true);
        new IPrimitivePropertyBuilder<TProperty> HasDbType(DbType dbType);
        new IPrimitivePropertyBuilder<TProperty> HasSize(int? size);
        new IPrimitivePropertyBuilder<TProperty> HasScale(byte? scale);
        new IPrimitivePropertyBuilder<TProperty> HasPrecision(byte? precision);
        new IPrimitivePropertyBuilder<TProperty> HasSequence(string name, string schema = null);
    }
}
