using System;
using System.Data;

namespace Dapper.Extensions.Expression.Infrastructure
{
    public class MappingType
    {
        public MappingType(Type type)
        {
            this.Type = type;
            this.DbValueConverter = new DbValueConverter(type);
            this.DbParameterAssembler = Infrastructure.DbParameterAssembler.Default;
        }
        public Type Type { get; private set; }
        public DbType DbType { get; set; } = DbType.Object;
        public IDbValueConverter DbValueConverter { get; set; }
        public IDbParameterAssembler DbParameterAssembler { get; set; }
    }
}
