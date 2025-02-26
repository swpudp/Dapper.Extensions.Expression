using System;
using System.Data;

namespace Dapper.Extensions.Expression.Utilities
{
    public static class TypeUtils
    {
        public static void UseStringGuidHandler()
        {
            SqlMapper.AddTypeHandler(new StringGuidHandler());
        }
    }

    public class StringGuidHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value)
        {
            return Guid.Parse(value.ToString());
        }

        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value.ToString();
        }
    }
}
