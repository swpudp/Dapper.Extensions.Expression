using System;
using System.Collections.Generic;

namespace Dapper.Extensions.Expression.WebTest.Model
{
    public class DoTask
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public TaskType TaskType { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        public int Version { get; set; }
    }

    public class DoTaskQuery
    {
        public string Name { get; set; }

        public TaskType? TaskType { get; set; }

        public DateTime? CreateTimeStart { get; set; }
        public DateTime? CreateTimeEnd { get; set; }

        public int Index { get; set; }
        public int Size { get; set; }
    }

    public class TaskGroupQuery
    {
        public string Name { get; set; }

        public DateTime? CreateTimeStart { get; set; }
        public DateTime? CreateTimeEnd { get; set; }

        public int Index { get; set; }
        public int Size { get; set; }
    }

    public class Paging<T> where T : class
    {
        public IList<T> Data { get; set; } = [];

        public int Index { get; set; } = 0;
        public int Size { get; set; } = 10;

        public int Total { get; set; } = 0;

        public int PageCount { get; set; } = 0;
    }
}
