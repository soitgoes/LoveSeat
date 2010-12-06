using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using LoveSeat.Interfaces;
using Newtonsoft.Json.Linq;

namespace LoveSeat
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
        bool? Stale { get; set; }
        int StartIndex { get; set; }
        int EndIndex { get; set; }
        int TotalRows { get; set; }
    }

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
        public int? Skip { get; set; }
        public bool? Stale { get; set; }
    }
    public static class PagingHelper
    {
        public static ViewOptions GetOptions(this IPageableModel model)
        {
            var options = new ViewOptions();
            options.Descending = model.Descending;
            options.SetStartKey(model.StartKey);
            options.Skip = model.Skip;
            options.Stale = model.Stale;
            options.Limit = model.Limit.HasValue ? model.Limit : 10;
            return options;
        }

        public static void UpdatePaging(this IPageableModel model, IViewOptions options, IViewResult result)
        {
            var limit = options.Limit.HasValue ? options.Limit.Value : 10;
            model.Limit = limit;
            int rowsMinusOffset = (result.TotalRows - result.OffSet);
            model.ShowPrev = result.OffSet != 0  && !(model.Descending && (rowsMinusOffset < options.Limit));
            model.ShowNext = (result.TotalRows > options.Limit + result.OffSet) || options.Descending.GetValueOrDefault();
            string skip = result.OffSet == 0 ? "" : "&skip=1";
            JToken lastRow; 
            JToken firstRow;  result.Rows.FirstOrDefault();
            if (options.Descending.HasValue && options.Descending.Value)
            {
                lastRow = result.Rows.FirstOrDefault();
                firstRow = result.Rows.LastOrDefault();
                model.StartIndex = rowsMinusOffset < limit ? 1 :  rowsMinusOffset - limit;
                
            }else
            {
                lastRow = result.Rows.LastOrDefault();
                firstRow = result.Rows.FirstOrDefault();
                model.StartIndex = result.TotalRows == 0 ? 0 :  result.OffSet +1;
            }
            var startIndexPlusLimit = model.StartIndex + limit;
            int count = result.Rows.Count();
            model.EndIndex =result.TotalRows == 0 ? 0 :  model.StartIndex + count - 1;
                     
            model.TotalRows = result.TotalRows;
            string prevStartKey = firstRow != null ? "&startkey=" + HttpUtility.UrlEncode(firstRow.Value<string>("key")) : "";
            string nextStartKey = lastRow != null ? "&startkey=" + HttpUtility.UrlEncode(lastRow.Value<string>("key") ): "";
            model.NextUrlParameters = "?limit=" + model.Limit  + nextStartKey  + skip  ;
            model.PrevUrlParameters = "?limit=" + model.Limit  + prevStartKey
                 + skip +
                "&descending=true";
        }
    }
}
