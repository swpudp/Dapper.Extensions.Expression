using System;
using Dapper.Extensions.Expression.Mapper;

namespace Dapper.Extensions.Expression.Query.Mapping
{
    public interface IObjectActivatorCreator
    {
        Type ObjectType { get; }
        bool IsRoot { get; set; }
        IObjectActivator CreateObjectActivator();
        IObjectActivator CreateObjectActivator(IDbContext dbContext);
        IFitter CreateFitter(IDbContext dbContext);
    }
}
