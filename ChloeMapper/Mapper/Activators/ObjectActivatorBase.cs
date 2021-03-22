using System.Data;
using System.Threading.Tasks;

namespace Dapper.Extensions.Expression.Mapper.Activators
{
    public abstract class ObjectActivatorBase : IObjectActivator
    {
        public virtual void Prepare(IDataReader reader)
        {

        }
        public abstract object CreateInstance(IDataReader reader);
        public virtual Task<object> CreateInstanceAsync(IDataReader reader)
        {
            return Task.FromResult(this.CreateInstance(reader));
        }
    }
}
