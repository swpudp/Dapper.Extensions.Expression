using System;

namespace Dapper.Extensions.Expression.Annotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class AutoIncrementAttribute : Attribute
    {
    }
}
