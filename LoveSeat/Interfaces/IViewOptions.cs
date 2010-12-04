using Newtonsoft.Json.Linq;

namespace LoveSeat.Interfaces
{
    public interface IViewOptions
    {
        JToken Key { get; set; }
        JToken StartKey { get; set; }
        JToken EndKey { get; set; }
        int? Limit { get; set; }
        int? Skip { get; set; }
        bool? Reduce { get; set; }
        bool? Group { get; set; }
        bool? IncludeDocs { get; set; }
        bool? InclusiveEnd { get; set; }
        int? GroupLevel { get; set; }
        bool? Descending { get; set; }
        bool? Stale { get; set; }
        string Etag { get; set; }
        void SetStartKey(string startKey);
        void SetEndKey(string endKey);
        void SetKey(string key);
        string ToString();
    }
}