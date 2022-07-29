using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Extensions.Expression
{
    /// <summary>
    /// 命名策略
    /// </summary>
    public enum NamingPolicy
    {
        None,
        CamelCase,
        LowerCase,
        SnakeCase,
        UpperCase,
        UpperSnakeCase
    }
}
