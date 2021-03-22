using Dapper.Extensions.Expression.Reflection;

namespace Dapper.Extensions.Expression.Mapper.Binders
{
    public class ComplexMemberBinder : MemberBinder, IMemberBinder
    {
        public ComplexMemberBinder(MemberValueSetter setter, IObjectActivator activtor) : base(setter, activtor)
        {
        }
    }
}
