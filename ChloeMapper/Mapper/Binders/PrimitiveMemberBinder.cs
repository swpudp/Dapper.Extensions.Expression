using System;
using System.Data;
using System.Reflection;
using Dapper.Extensions.Expression.Reflection;

namespace Dapper.Extensions.Expression.Mapper.Binders
{
    public class PrimitiveMemberBinder : IMemberBinder
    {
        MemberInfo _member;
        MRMTuple _mrmTuple;
        int _ordinal;
        IMRM _mMapper;

        public PrimitiveMemberBinder(MemberInfo member, MRMTuple mrmTuple, int ordinal)
        {
            this._member = member;
            this._mrmTuple = mrmTuple;
            this._ordinal = ordinal;
        }

        public int Ordinal { get { return this._ordinal; } }

        public void Prepare(IDataReader reader)
        {
            Type fieldType = reader.GetFieldType(this._ordinal);
            if (fieldType == this._member.GetMemberType().GetUnderlyingType())
            {
                this._mMapper = this._mrmTuple.StrongMRM.Value;
                return;
            }

            this._mMapper = this._mrmTuple.SafeMRM.Value;
        }
        public void Bind(object obj, IDataReader reader)
        {
            this._mMapper.Map(obj, reader, this._ordinal);
        }
    }
}
