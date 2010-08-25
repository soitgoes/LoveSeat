namespace LoveSeat.Support
{
	public class ReplicationOptions
	{
		private readonly string source;
		private readonly string target;
		private readonly bool continuous;
		private readonly string query_params;

		public ReplicationOptions(string source, string target, bool continuous, string query_params)
		{
			this.source = source;
			this.target = target;
			this.continuous = continuous;
			this.query_params = query_params;
		}
		public ReplicationOptions(string source, string target, bool continuous) : this(source, target, continuous, null)
		{
		}
		public ReplicationOptions(string source, string target) : this(source, target, false)
		{
		}

		public override string ToString()
		{
			string result = @"{ ""source"": ""%source%"", ""target"" : ""%target%"", ""continuous"" : %continuous% " +
			                (string.IsNullOrEmpty(query_params) ? "" : @", ""query_params"" : %query_params%") +
			                " }";
			result = result.Replace("%source%", source).Replace("%target%", target).Replace("%query_params%", query_params).Replace("%continuous%", continuous.ToString().ToLower());
			return result;
		}
	}
}