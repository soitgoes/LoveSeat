namespace LoveSeat.Core.Support
{
	public class ReplicationOptions
	{
		private readonly string _source;
		private readonly string _target;
		private readonly bool _continuous;
		private readonly string _queryParams;

		public ReplicationOptions(string source, string target, bool continuous, string query_params)
		{
			_source = source;
			_target = target;
			_continuous = continuous;
			_queryParams = query_params;
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
			                (string.IsNullOrEmpty(_queryParams) ? "" : @", ""query_params"" : %query_params%") +
			                " }";

			result = result
                .Replace("%source%", _source)
                .Replace("%target%", _target)
                .Replace("%query_params%", _queryParams)
                .Replace("%continuous%", _continuous.ToString()
                .ToLower());

			return result;
		}
	}
}