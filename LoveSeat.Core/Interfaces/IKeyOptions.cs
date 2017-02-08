using Newtonsoft.Json.Linq;

namespace LoveSeat.Core.Interfaces
{
    public interface IKeyOptions
    {
        string ToString();
        string ToRawString();
        void Insert(int index, JToken item);
        void RemoveAt(int index);
        void Add(JToken item);
        bool Remove(JToken item);
        int Count { get; }
        bool HasValues { get; }
        void Add(object content);
        void Add(params object[] items);
    }
}
