using System.Collections.Generic;

namespace Dapper.Extensions.Expression.Utility
{
    class KeyValuePairList<TKey, TValue> : List<KeyValuePair<TKey, TValue>>
    {
        public KeyValuePairList()
        {
        }
        public void Add(TKey key, TValue value)
        {
            this.Add(new KeyValuePair<TKey, TValue>(key, value));
        }
    }
}
