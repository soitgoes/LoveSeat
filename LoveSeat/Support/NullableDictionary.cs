using System.Collections.Generic;

namespace LoveSeat.Support
{
    public class CouchDictionary<Y> : NullableDictionary<string, Y>
    {      
    }

    public class NullableDictionary<X, Y> 
    {
        private readonly Dictionary<X,Y> dictionary  = new Dictionary<X, Y>();
        public NullableDictionary()
        {        
        }
        public void Add(X key, Y value)
        {
            dictionary.Add(key, value);
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