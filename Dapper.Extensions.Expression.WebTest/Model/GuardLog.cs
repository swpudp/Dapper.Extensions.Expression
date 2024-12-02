using System;

namespace Dapper.Extensions.Expression.WebTest.Model
{
    /// <summary>
    /// 门禁日志
    /// </summary>
    public class GuardLog
    {
        public required string Id { get; set; }

        public required string OwnerId { get; set; }

        public DateTime OpenTime { get; set; }

        public GuardMode OpenMode { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        public int Version { get; set; }
    }

    public class AddGuardLogReq
    {
        public string OwnerId { get; set; }

        public DateTime OpenTime { get; set; }

        public GuardMode OpenMode { get; set; }
    }
}
