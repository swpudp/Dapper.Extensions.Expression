using System;
using System.Collections.Generic;
using System.Data;
using Dapper.Extensions.Expression.Data;
using Dapper.Extensions.Expression.Descriptors;

namespace Dapper.Extensions.Expression.Mapper
{
    public interface IEntityRowComparer
    {
        bool IsEntityRow(object entity, IDataReader reader);
    }
    public class EntityRowComparer : IEntityRowComparer
    {
        List<Tuple<PropertyDescriptor, int, IDbValueReader>> _keys;
        object _entity;
        object[] _keyValues;
        public EntityRowComparer(List<Tuple<PropertyDescriptor, int>> keys)
        {
            List<Tuple<PropertyDescriptor, int, IDbValueReader>> keyList = new List<Tuple<PropertyDescriptor, int, IDbValueReader>>(keys.Count);
            for (int i = 0; i < keys.Count; i++)
            {
                var tuple = keys[i];
                IDbValueReader dbValueReader = DataReaderConstant.GetDbValueReader(tuple.Item1.PropertyType);

                keyList.Add(new Tuple<PropertyDescriptor, int, IDbValueReader>(tuple.Item1, tuple.Item2, dbValueReader));
            }

            this._keys = keyList;
            this._keyValues = new object[keys.Count];
        }

        public bool IsEntityRow(object entity, IDataReader reader)
        {
            object[] keyValues = this.GetKeyValues(entity);

            for (int i = 0; i < this._keys.Count; i++)
            {
                object keyValue = keyValues[i];

                if (keyValue == null)
                    return false;

                var tuple = this._keys[i];
                int ordinal = tuple.Item2;
                var dbValueReader = tuple.Item3;
                object keyReaderValue = dbValueReader.GetValue(reader, ordinal);

                if (keyReaderValue == null)
                    return false;

                if (!keyValue.Equals(keyReaderValue))
                    return false;
            }

            return true;
        }

        object[] GetKeyValues(object entity)
        {
            if (object.ReferenceEquals(entity, this._entity))
            {
                return this._keyValues;
            }

            for (int i = 0; i < this._keys.Count; i++)
            {
                var tuple = this._keys[i];
                PropertyDescriptor propertyDescriptor = tuple.Item1;
                object keyValue = propertyDescriptor.GetValue(entity);

                this._keyValues[i] = keyValue;
            }

            this._entity = entity;

            return this._keyValues;
        }
    }
}
