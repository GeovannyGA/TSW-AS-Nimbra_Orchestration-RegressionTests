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

    public enum WorkOrderStatus
    {
        Na = -1,
        Created = 1,
        Failed = 2,
        Complete = 3,
        Canceled = 4,
        InProgress = 5,
        PendingUpdate = 6,
        UpdateSent = 7,
    }
}
