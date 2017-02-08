using LoveSeat.Core.Interfaces;

namespace LoveSeat.Core
{
    public class PageableModel : IPageableModel
    {
        public bool ShowNext { get; set; }
        public bool ShowPrev { get; set; }
        public string NextUrlParameters { get; set; }
        public string PrevUrlParameters { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int TotalRows { get; set; }
        public int? Limit { get; set; }
        public bool Descending { get; set; }
        public string StartKey { get; set; }
        public string StartKeyDocId { get; set; }
        public string EndKeyDocId { get; set; }
        public int? Skip { get; set; }
        public string StaleOption { get; set; }
        public bool? Stale { get; set; }
    }
}
