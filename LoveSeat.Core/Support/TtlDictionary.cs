using System;
using System.Collections.Generic;

namespace LoveSeat.Core.Support
{
    internal class TtlDictionary<X, Y>
    {
        private readonly Dictionary<X, Y> _items;
        private readonly Dictionary<X, DateTime> _expiration;

        public TtlDictionary()
        {
            _items = new Dictionary<X, Y>();
            _expiration = new Dictionary<X, DateTime>();
        }

        public void Add(X key, Y value, TimeSpan ttl)
        {
            if (_items.ContainsKey(key))
            {
                _items.Remove(key);
                _expiration.Remove(key);
            }

            _items.Add(key, value);
            _expiration.Add(key, DateTime.Now.Add(ttl));
            RemoveExpiredKeys();
        }

        private void RemoveExpiredKeys()
        {
            foreach (var key in _expiration.Keys)
            {
                if (_expiration[key] < DateTime.Now)
                {
                    _expiration.Remove(key);
                    _items.Remove(key);
                }
            }
        }

        public Y this[X key]
        {
            get
            {
                if (_expiration.ContainsKey(key) && _expiration[key] > DateTime.Now)
                    return _items[key];
                else
                    return default(Y);
            }
        }
    }
}
