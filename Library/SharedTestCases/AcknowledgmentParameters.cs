namespace Library.SharedTestCases
{
	using System;

	public class AcknowledgmentParameters
	{
		public DateTime Start { get; set; }

		public DateTime End { get; set; }

		public string JobName { get; set; }

		public string Source { get; set; }

		public string Destination { get; set; }

		public string SourceGroup { get; set; }

		public string DestinationGroup { get; set; }

		public string Platform { get; set; }

		public string Endpoint { get; set; } = "http://172.16.100.5:8200";

		public string WorkOrder { get; set; }

		public string ChainId { get; set; }
	}
}
