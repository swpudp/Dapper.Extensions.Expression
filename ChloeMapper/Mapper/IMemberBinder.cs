using System.Data;

namespace Dapper.Extensions.Expression.Mapper
{
    public interface IMemberBinder
    {
        void Prepare(IDataReader reader);
        void Bind(object obj, IDataReader reader);
    }
}
