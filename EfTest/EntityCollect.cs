using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EfTest
{
    public enum TestType
    {
        Latest,

        Later
    }

    public static class TestTool
    {
        public static bool IsDelete(Log log)
        {
            return log.Name.Contains("a");
        }
    }

    public class Log
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime Logged { get; set; }

        public TestType TestType { get; set; }

        public int? ThreadId { get; set; }

        public bool IsDelete { get; set; }

        public DateTime? UpdateTime { get; set; }

        public int Version { get; set; }
    }

    [Table("order")]
    public class Order
    {
        public Guid Id { get; set; }

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

        public DateTime CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// ÐòºÅ
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// °æ±¾ºÅ
        /// </summary>
        public int Version { get; set; }
    }

    public enum Status
    {
        Draft,
        Running,
        Stop
    }

    public enum SignState
    {
        UnSign,
        Signed
    }
}
