namespace LoveSeat.Core.Interfaces
{
    public interface IPageableModel
    {
        bool ShowNext { get; set; }
        bool ShowPrev { get; set; }
        string NextUrlParameters { get; set; }
        string PrevUrlParameters { get; set; }
        int? Limit { get; set; }
        bool Descending { get; set; }
        string StartKey { get; set; }
        int? Skip { get; set; }
        string StaleOption { get; set; }
        bool? Stale { get; set; }
        int StartIndex { get; set; }
        int EndIndex { get; set; }
        int TotalRows { get; set; }
        string StartKeyDocId { get; set; }
        string EndKeyDocId { get; set; }
    }
}
