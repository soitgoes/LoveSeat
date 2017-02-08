using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Net;
using LoveSeat.Core.Interfaces;

namespace LoveSeat.Core
{
    public class KeyOptions : IKeyOptions
    {
        private JArray _objects = new JArray();

        public KeyOptions(params object[] objects)
        {
            foreach (var obj in objects)
            {
                Add(obj);
            }
        }

        public KeyOptions(JArray jArray)
        {
            _objects = jArray;
        }

        public override string ToString()
        {
            if (_objects.Count == 0)
                return string.Empty;

            if (_objects.Count == 1)
            {
                return WebUtility.UrlEncode(_objects[0].ToString(Formatting.None, new IsoDateTimeConverter()));
            }

            bool first = true;
            string result = "[";
            foreach (var item in _objects)
            {
                if (!first)
                    result += ",";
                first = false;
                if (item.ToString().Equals("{}"))
                    result += item.ToString(Formatting.None, new IsoDateTimeConverter());
                else
                    result += WebUtility.UrlEncode(item.ToString(Formatting.None, new IsoDateTimeConverter()));
            }
            result += "]";

            return result;
        }

        public string ToRawString()
        {
            if (_objects.Count == 0)
                return string.Empty;

            if (_objects.Count == 1)
                return _objects[0].ToString(Formatting.None, new IsoDateTimeConverter());

            bool first = true;
            string result = "[";
            foreach (var item in _objects)
            {
                if (!first)
                    result += ",";

                first = false;
                result += item.ToString(Formatting.None, new IsoDateTimeConverter());
            }

            result += "]";
            return result;
        }

        public void Insert(int index, JToken item)
        {
            _objects.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _objects.RemoveAt(index);
        }

        public bool Remove(JToken item)
        {
            return _objects.Remove(item);
        }

        public int Count
        {
            get { return _objects.Count; }
        }

        public bool HasValues
        {
            get { return _objects.Count > 0; }
        }

        public void Add(JToken item)
        {
            _objects.Add(item);
        }

        public void Add(object item)
        {
            if (item == CouchValue.MaxValue)
            {
                _objects.Add(new JRaw("{}"));
                return;
            }

            _objects.Add(item);
        }

        public void Add(params object[] items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }
    }
}
