using System.Collections.Generic;

namespace Dapper.Extensions.Expression.Core
{
    public class DbCommandInfo
    {
        public string CommandText { get; set; }
        public List<DbParam> Parameters { get; set; }

        public DbParam[] GetParameters()
        {
            return this.Parameters.ToArray();
        }
    }
}
