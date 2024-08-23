namespace Dapper.Extensions.Expression
{
    /// <summary>
    /// 命名策略
    /// </summary>
    public enum NamingPolicy
    {
        /// <summary>
        /// abcAbc
        /// </summary>
        CamelCase,
        /// <summary>
        /// abcabc
        /// </summary>
        LowerCase,
        /// <summary>
        /// abc_abc
        /// </summary>
        SnakeCase,
        /// <summary>
        /// ABCABC
        /// </summary>
        UpperCase,
        /// <summary>
        /// ABC_ABC
        /// </summary>
        UpperSnakeCase
    }
}
