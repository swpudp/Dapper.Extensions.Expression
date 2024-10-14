using System;

namespace Dapper.Extensions.Expression.UnitTests
{
    //[TableNaming(NamingPolicy.UpperCase), FieldNaming(NamingPolicy.UpperCase)]
    public class Buyer : IEntity
    {
        [Key] public Guid Id { get; set; }

        public string Name { get; set; }

        public BuyerType Type { get; set; }

        public string Code { get; set; }

        public string Identity { get; set; }

        public string Email { get; set; }

        public string Mobile { get; set; }

        public bool IsDelete { get; set; }

        public bool? IsActive { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }
    }

    [Table("items")]
    //[TableNaming(NamingPolicy.UpperCase), FieldNaming(NamingPolicy.UpperCase)]
    public class Item : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public int Index { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public decimal Quantity { get; set; }

        public decimal? Discount { get; set; }

        public decimal Amount { get; set; }

        public string Unit { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }
    }

    //[TableNaming(NamingPolicy.UpperCase), FieldNaming(NamingPolicy.UpperCase)]
    public class Attachment : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public string Name { get; set; }

        public int Enable { get; set; }

        public string Extend { get; set; }

        public int Version { get; set; }
    }

    //[TableNaming(NamingPolicy.UpperCase), FieldNaming(NamingPolicy.UpperCase)]
    public class Order : IEntity
    {
        [Key] public Guid Id { get; set; }

        public Guid BuyerId { get; set; }

        [Column("Number")]
        public string SerialNo { get; set; }

        public string Remark { get; set; }

        public Status Status { get; set; }

        public SignState? SignState { get; set; }

        public decimal Amount { get; set; }
        public decimal? Freight { get; set; }

        public Guid? DocId { get; set; }

        public bool IsDelete { get; set; }

        public bool? IsActive { get; set; }

        public bool IsEnable { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        [Computed]
        public int Index { get; set; }

        [NotMapped]
        public LogType Type { get; set; }

        [NotMapped]
        public string Ignore { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }
    }

    //[TableNaming(NamingPolicy.UpperCase), FieldNaming(NamingPolicy.UpperCase)]
    public class Emit : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }
    }

    //[TableNaming(NamingPolicy.UpperCase), FieldNaming(NamingPolicy.UpperCase)]
    public class Log : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime Logged { get; set; }

        public LogType LogType { get; set; }

        public int Version { get; set; }
    }

    /// <summary>
    /// 测试命名策略
    /// </summary>
    //[TableNaming(NamingPolicy.UpperSnakeCase), FieldNaming(NamingPolicy.UpperSnakeCase)]
    public class NamingPolicySnakeCase : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        public int Version { get; set; }

        public NamingPolicy NamingType { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
