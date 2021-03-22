using Dapper.Extensions.Expression.Reflection;

namespace Dapper.Extensions.Expression.Mapper.Binders
{
    public class CollectionMemberBinder : MemberBinder, IMemberBinder
    {
        public CollectionMemberBinder(MemberValueSetter setter, IObjectActivator activtor) : base(setter, activtor)
        {

        }
    }
}
