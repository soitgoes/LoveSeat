using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoveSeat.Support
{
	public class TtlDictionary<X, Y>
    {
        private readonly Dictionary<X, Y> items;
        private readonly Dictionary<X, DateTime> expiration;

        public TtlDictionary()
        {
            items = new Dictionary<X, Y>();
            expiration = new Dictionary<X, DateTime>();
        }

    	public Dictionary<X, Y> Items
    	{
    		get { return items; }
    	}

    	public Dictionary<X, DateTime> Expiration
    	{
    		get { return expiration; }
    	}

        public void Add(X key, Y value, TimeSpan ttl)
        {
            if (Items.ContainsKey(key))
            {
                Items.Remove(key);
                Expiration.Remove(key);
            }
            Items.Add(key, value);
            Expiration.Add(key, DateTime.Now.Add(ttl));
            RemoveExpiredKeys();
        }

        private void RemoveExpiredKeys()
        {
            foreach (var key in Expiration.Keys)
            {
                if (Expiration[key] < DateTime.Now)
                {
                    Expiration.Remove(key);
                    Items.Remove(key);
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
