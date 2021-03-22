using System.Data;
using System.Threading.Tasks;

namespace Dapper.Extensions.Expression.Mapper
{
    public interface IObjectActivator
    {
        void Prepare(IDataReader reader);
        object CreateInstance(IDataReader reader);
        Task<object> CreateInstanceAsync(IDataReader reader);
    }
}
