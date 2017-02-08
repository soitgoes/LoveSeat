using LoveSeat.Core.Interfaces;

namespace LoveSeat.Core
{
    public class Document<T> : Document where T : class
    {
        private static IObjectSerializer _objectSerializer = new DefaultSerializer();

        public Document(T obj)
            : base(_objectSerializer.Serialize(obj))
        {
        }

        public Document(T obj, IObjectSerializer objectSerializer)
            : base(objectSerializer.Serialize(obj))
        {
        }

        public Document(string json)
            : base(json)
        {

        }

        public void UpdateFromItem(T item)
        {
            _jObject.CopyFromObj(item);
        }

        public T Item
        {
            get { return _jObject.ToObject<T>(); }
        }
    }
}