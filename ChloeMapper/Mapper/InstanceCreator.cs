using System.Collections.Generic;
using System.Data;

namespace Dapper.Extensions.Expression.Mapper
{
    public delegate object InstanceCreator(IDataReader reader, List<IObjectActivator> argumentActivators);
}
