using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoveSeat.Support
{
	public class TtlDictionary<X, Y>
    {
        private readonly ConcurrentDictionary<X, Y> items;
        private readonly ConcurrentDictionary<X, DateTime> expiration;

        public TtlDictionary()
        {
            items = new ConcurrentDictionary<X, Y>();
            expiration = new ConcurrentDictionary<X, DateTime>();
        }

        public ConcurrentDictionary<X, Y> Items
    	{
    		get { return items; }
    	}

        public ConcurrentDictionary<X, DateTime> Expiration
    	{
    		get { return expiration; }
    	}

        public void Add(X key, Y value, TimeSpan ttl)
        {
            if (Items.ContainsKey(key))
            {
                var removed = default(Y);
                Items.TryRemove(key, out removed);

                DateTime removeDateTime;
                Expiration.TryRemove(key, out removeDateTime);
            }
            Items.TryAdd(key, value);
            Expiration.TryAdd(key, DateTime.Now.Add(ttl));
            RemoveExpiredKeys();
        }

        private void RemoveExpiredKeys()
        {
            foreach (var key in Expiration.Keys)
            {
                if (Expiration[key] < DateTime.Now)
                {
                    var removed = default(Y);

                    DateTime removeDateTime;

                    Expiration.TryRemove(key, out removeDateTime);
                    Items.TryRemove(key, out removed);
                }
            }
        }
        public Y this[X key]
        {
            get
            {
                if (Expiration.ContainsKey(key) && Expiration[key] > DateTime.Now)
                {
                    return Items[key];
                }
                else
                {
                    return default(Y);
                }
            }
        }

    }
}
