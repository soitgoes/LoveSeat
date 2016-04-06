using System;
using System.Linq;

namespace LoveSeat.Support
{
	public class ReplicationOptions
	{
		private readonly string source;
		private readonly string target;
		private readonly bool continuous;
		private readonly string query_params;
        private readonly string[] doc_ids;

		public ReplicationOptions(string source, string target, bool continuous, string query_params, string[] doc_ids)
		{
			this.source = source;
			this.target = target;
			this.continuous = continuous;
			this.query_params = query_params;
            this.doc_ids = doc_ids;
		}
		public ReplicationOptions(string source, string target, bool continuous) : this(source, target, continuous, null, null)
		{
		}
        public ReplicationOptions(string source, string target, bool continuous, string[] doc_ids) : this(source, target, continuous, null, doc_ids)
        {
        }
        public ReplicationOptions(string source, string target) : this(source, target, false)
		{
		}

		public override string ToString()
		{
			string result = @"{ ""source"": ""%source%"", ""target"" : ""%target%"", ""continuous"" : %continuous% " +
			                (string.IsNullOrEmpty(query_params) ? "" : @", ""query_params"" : %query_params%") +
                            (doc_ids == null || doc_ids.Length == 0 ? "" : @", ""doc_ids"" : %doc_ids%") + 
			                " }";
			result = result
                .Replace("%source%", source)
                .Replace("%target%", target)
                .Replace("%query_params%", query_params)
                .Replace("%continuous%", continuous.ToString().ToLower());

            if(doc_ids != null && doc_ids.Length > 0)
            {
                var array = string.Join(",", doc_ids.Select(x => x.Replace(x, "\"" + x + "\"")));
                result = result.Replace("%doc_ids%", string.Format("[{0}]", array));
            }

			return result;
		}
	}
}