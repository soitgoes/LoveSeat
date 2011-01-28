using System.Collections.Generic;

namespace LoveSeat.Support
{
    public class NullableDictionary<X, Y>
    {
        private Dictionary<X, Y> dictionary = new Dictionary<X, Y>();
        public NullableDictionary(Dictionary<X, Y> dictionary)
        {
            this.dictionary = dictionary;
        }
        public Y this[X key]
        {
            get
            {
                if (dictionary.ContainsKey(key))
                {
                    return dictionary[key];
                }
                else
                {
                    return default(Y);
                }
            }
        }


    }
}