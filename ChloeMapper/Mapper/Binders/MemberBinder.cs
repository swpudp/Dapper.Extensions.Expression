using System.Data;
using Dapper.Extensions.Expression.Reflection;

namespace Dapper.Extensions.Expression.Mapper.Binders
{
    public class MemberBinder : IMemberBinder
    {
        MemberValueSetter _setter;
        IObjectActivator _activtor;
        public MemberBinder(MemberValueSetter setter, IObjectActivator activtor)
        {
            this._setter = setter;
            this._activtor = activtor;
        }
        public virtual void Prepare(IDataReader reader)
        {
            this._activtor.Prepare(reader);
        }
        public virtual void Bind(object obj, IDataReader reader)
        {
            object val = this._activtor.CreateInstance(reader);
            this._setter(obj, val);
        }
    }
}
