using System.Linq;
using Newtonsoft.Json.Linq;
using System.Net;
using LoveSeat.Core.Interfaces;

namespace LoveSeat.Core
{
    public static class PagingHelper
    {
        public static ViewOptions GetOptions(this IPageableModel model)
        {
            var options = new ViewOptions();
            options.Descending = model.Descending;
            options.StartKey.Add(model.StartKey);
            options.Skip = model.Skip;
            options.Stale = model.Stale;
            options.StartKeyDocId = model.StartKeyDocId;
            options.EndKeyDocId = model.EndKeyDocId;
            options.Limit = model.Limit.HasValue ? model.Limit : 10;

            return options;
        }

        public static void UpdatePaging(this IPageableModel model, IViewOptions options, IViewResult result)
        {
            int count = result.Rows.Count();
            var limit = options.Limit.HasValue ? options.Limit.Value : 10;
            model.Limit = limit;
            int rowsMinusOffset = (result.TotalRows - result.OffSet);
            model.ShowPrev = result.OffSet != 0 && !(model.Descending && (rowsMinusOffset <= count));
            model.ShowNext = (result.TotalRows > options.Limit + result.OffSet) || options.Descending.GetValueOrDefault();
            string skipPrev = result.OffSet < limit ? "" : "&skip=1";
            string skipNext = rowsMinusOffset < limit ? "" : "&skip=1";
            JToken lastRow;
            JToken firstRow;

            if (options.Descending.HasValue && options.Descending.Value)
            {   
                //descending
                lastRow = result.Rows.FirstOrDefault();
                firstRow = result.Rows.LastOrDefault();
                model.StartIndex = (rowsMinusOffset - limit) < 1 ? 1 : (rowsMinusOffset - limit + 1);
            }
            else
            {   
                //ascending
                lastRow = result.Rows.LastOrDefault();
                firstRow = result.Rows.FirstOrDefault();
                model.StartIndex = result.OffSet + 1;
            }

            var startIndexPlusCount = model.StartIndex + count - 1;
            model.EndIndex = result.TotalRows == 0 ? 0 : startIndexPlusCount;
            if (count == 0) model.EndIndex = model.StartIndex = 0;

            model.TotalRows = result.TotalRows;
            string prevStartKey = firstRow != null ? "&startkey=" + WebUtility.UrlEncode(firstRow.Value<string>("key")) + "&StartKeyDocId=" + firstRow.Value<string>("id") : "";
            string nextStartKey = lastRow != null ? "&startkey=" + WebUtility.UrlEncode(lastRow.Value<string>("key")) + "&StartKeyDocId=" + lastRow.Value<string>("id") : "";
            model.NextUrlParameters = "?limit=" + model.Limit + nextStartKey + skipNext;
            model.PrevUrlParameters = "?limit=" + model.Limit + prevStartKey
                 + skipPrev +
                "&descending=true";
        }
    }
}
