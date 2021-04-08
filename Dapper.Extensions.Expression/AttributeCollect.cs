using System;

namespace Dapper.Extensions.Expression
{
    /// <summary>
    /// Defines the name of a table to use in Dapper.Contrib commands.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// Creates a table mapping to a specific name for Dapper.Contrib commands
        /// </summary>
        /// <param name="tableName">The name of this table in the database.</param>
        public TableAttribute(string tableName)
        {
            Name = tableName;
        }

        /// <summary>
        /// The name of the table in the database
        /// </summary>
        public string Name { get; }
    }

    /// <summary>
    /// Specifies that this field is a primary key in the database
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : Attribute
    {
    }

    /// <summary>
    /// Specifies that this field is an explicitly set primary key in the database
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ExplicitKeyAttribute : Attribute
    {
    }

    /// <summary>
    /// 自动计算字段，不可写入和更新，可查询
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ComputedAttribute : Attribute
    {
    }

    /// <summary>
    /// 未映射字段，不可写入、更新、查询
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NotMappedAttribute : Attribute
    {
    }

    /// <summary>
    /// 列名称指定
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute(string columnName)
        {
            ColumnName = columnName;
        }

        /// <summary>
        /// 列名
        /// </summary>
        public string ColumnName { get; }
    }

    public enum JoinType
    {
        Left,
        Right,
        Inner,
        Full
    }
}
