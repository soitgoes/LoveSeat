using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using LoveSeat.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace LoveSeat
{
    public class KeyOptions : IKeyOptions
    {
        private JArray objects = new JArray();

        public KeyOptions(params object[] objects)
        {
            foreach (var obj in objects)
            {
                this.Add(obj);
            }
        }

        public KeyOptions(JArray jArray)
        {
            this.objects = jArray;
        }

        public override string ToString()
        {
            if (objects.Count == 0)
            {
                return "";
            }
            if (objects.Count == 1)
            {
                return HttpUtility.UrlEncode(objects[0].ToString(Formatting.None, new IsoDateTimeConverter()));
            }
            
            string result = "[";
            bool first = true;
            foreach (var item in objects)
            {
                if (!first)
                    result += ",";
                first = false;
                if(item.ToString().Equals("{}"))
                    result += item.ToString(Formatting.None, new IsoDateTimeConverter());
                else
                    result += HttpUtility.UrlEncode(item.ToString(Formatting.None, new IsoDateTimeConverter()));
            }
            result += "]";
            return result;
        }

        public string ToRawString() 
        {
          if (objects.Count == 0) {
            return "";
          }
          if (objects.Count == 1) {
            return objects[0].ToString(Formatting.None, new IsoDateTimeConverter());
          }

          string result = "[";
          bool first = true;
          foreach (var item in objects) {
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
            objects.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            objects.RemoveAt(index);
        }

        public bool Remove(JToken item)
        {
            return objects.Remove(item);
        }

        public int Count
        {
            get { return objects.Count; }
        }

        public bool HasValues
        {
            get { return objects.Count > 0; }
        }

        public void Add(JToken item)
        {
            objects.Add(item);
        }

        public void Add(object item)
        {
            if (item == CouchValue.MaxValue)
            {
                objects.Add(new JRaw("{}"));
                return;
            }
            objects.Add(item);
        }

        public void Add(params object[] items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }
    }
    public static class CouchValue
    {
        static object value = new object();
        public static object MaxValue
        {
            get
            {
                return value;
            }
        }

        public static object MinValue
        {
            get { return null; }
        }
    }
}
