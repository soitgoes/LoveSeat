using System.Collections.Generic;

namespace LoveSeat.Core.Support
{
    public class NullableDictionary<X, Y>
    {
        private readonly Dictionary<X, Y> _dictionary = new Dictionary<X, Y>();

        public NullableDictionary()
        {
        }

        public void Add(X key, Y value)
        {
            _dictionary.Add(key, value);
        }

        public Y this[X key]
        {
            get
            {
                if (_dictionary.ContainsKey(key))
                    return _dictionary[key];
                else
                    return default(Y);
            }
        }
    }
}