using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LoveSeat.Core.Interfaces;
using LoveSeat.Core.Support;

namespace LoveSeat.Core
{
    public class ViewResult<T> : ViewResult, IViewResult<T> where T : class
    {
        private readonly IObjectSerializer _objectSerializer = null;
        private CouchDictionary<T> _dict = null;

        public ViewResult(CouchResponse response, HttpWebRequest request, IObjectSerializer objectSerializer, bool includeDocs = false)
            : base(response, request, includeDocs)
        {
            _objectSerializer = objectSerializer;
        }

        public CouchDictionary<T> Dictionary
        {
            get
            {
                if (_dict != null) return _dict;

                _dict = new CouchDictionary<T>();
                foreach (var row in Rows)
                {
                    _dict.Add(row.Value<JToken>("key").ToString(Formatting.None), _objectSerializer.Deserialize<T>(row.Value<string>("value")));
                }

                return _dict;
            }
        }

        public IEnumerable<T> Items
        {
            get
            {
                if (_objectSerializer == null)
                {
                    throw new InvalidOperationException("ObjectSerializer must be set in order to use the generic view.");
                }

                var values = IncludeDocs ? RawDocs : RawValues;
                return values.Select(item => _objectSerializer.Deserialize<T>(item));
            }
        }
    }
}