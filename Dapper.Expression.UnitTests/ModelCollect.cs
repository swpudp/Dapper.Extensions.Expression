using System;

namespace Dapper.Extensions.Expression.UnitTests
{

    public class QueryParam
    {
        public DateTime? CreateTime { get; set; }

        public bool? IsDelete { get; set; }

        public string Key { get; set; }
    }

    public class OrderModel : IEntity
    {
        public Guid Id { get; set; }

        public Guid BuyerId { get; set; }

        public string Number { get; set; }

        public string Remark { get; set; }

        public Status Status { get; set; }

        public string StatusDesc => Status.ToString();

        public SignState? SignState { get; set; }

        public string SignStateDesc => SignState.ToString();

        public decimal Amount { get; set; }
        public decimal? Freight { get; set; }

        public Guid? DocId { get; set; }

        public bool IsDelete { get; set; }

        public bool? IsActive { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        public LogType Type { get; set; }

        public string TypeDesc => Type.ToString();

        public string Ignore { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }
    }

    public class OrderAliasModel : IEntity
    {
        public Guid Id { get; set; }

        public Guid A { get; set; }

        public string B { get; set; }

        public string C { get; set; }

        public Status D { get; set; }

        public string StatusDesc => D.ToString();

        public SignState? E { get; set; }

        public string SignStateDesc => E.ToString();

        public decimal F { get; set; }
        public decimal? G { get; set; }

        public Guid? H { get; set; }

        public bool I { get; set; }

        public bool? J { get; set; }

        public DateTime K { get; set; }

        public DateTime? L { get; set; }

        public LogType M { get; set; }

        public string TypeDesc => M.ToString();

        public string N { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }
    }

    public interface IEntity
    {
        Guid Id { get; set; }

        int Version { get; set; }
    }

    public enum LogType
    {
        Log,
        Trace
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

    public enum BuyerType
    {
        Person,
        Company,
        Other
    }

    public static class TestTool
    {
        public static bool IsDelete(QueryParam param)
        {
            return param.Key.Contains("a");
        }
    }
}
