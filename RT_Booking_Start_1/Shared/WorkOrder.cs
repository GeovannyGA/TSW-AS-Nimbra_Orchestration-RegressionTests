namespace RT_Booking_Start_1.Shared
{
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Text;

	public enum WorkOrderInteropStatus
	{
		Confirmed = 1,
		Pending = 2,
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

	public class WorkOrder
	{
		public string InstanceId { get; set; }

		public string WorkOrderId { get; set; }

		public string ChainId { get; set; }

		public DateTime? StartTime { get; set; }

		public DateTime? EndTime { get; set; }

		public int MessageType { get; set; }

		public string Custom1 { get; set; }

		public string Custom2 { get; set; }

		public string Custom3 { get; set; }

		public string Custom4 { get; set; }

		public string Custom5 { get; set; }

		public string Custom6 { get; set; }

		public string Custom7 { get; set; }

		public string Custom8 { get; set; }

		public int Custom9 { get; set; }

		public int Custom10 { get; set; }

		public int InteropStatus { get; set; }

		public double UpdateOADate { get; set; }

		public int BookingStatus { get; set; }

		public int FailureDescription { get; set; }

		public string DisplayKey { get; set; }

		public string Message { get; private set; }

		public string PreviousJobName { get; set; }

		public object[] ToObjectArray()
		{
			return new object[]
			{
					InstanceId,
					WorkOrderId,
					ChainId,
					StartTime,
					EndTime,
					MessageType,
					Custom1,
					Custom2,
					Custom3,
					Custom4,
					Custom5,
					Custom6,
					Custom7,
					Custom8,
					Custom9,
					Custom10,
					InteropStatus,
					UpdateOADate,
					BookingStatus,
					FailureDescription,
					Message,
					DisplayKey,
					PreviousJobName,
			};
		}

		public void BuildMessage()
		{
			string FormatDate(DateTime? dt) =>
				dt.HasValue ? dt.Value.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) : "";

			string now = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

			var sb = new StringBuilder();
			sb.AppendLine("<InteropSetup>");
			sb.AppendLine($"  <MessageType>New</MessageType>");
			sb.AppendLine($"  <CircuitID>{ChainId}</CircuitID>");
			sb.AppendLine($"  <SharedID>18923025</SharedID>");
			sb.AppendLine($"  <WorkOrder>{WorkOrderId}</WorkOrder>");
			sb.AppendLine($"  <Client>{Custom4}</Client>");
			sb.AppendLine($"  <JobName>{Custom3}</JobName>");
			sb.AppendLine($"  <Start>{FormatDate(StartTime)}</Start>");
			sb.AppendLine($"  <End>{FormatDate(EndTime)}</End>");
			sb.AppendLine($"  <ServiceDescription>{Custom5}</ServiceDescription>");
			sb.AppendLine($"  <ServiceID>{Custom6}</ServiceID>");
			sb.AppendLine($"  <Platform>Test</Platform>");
			sb.AppendLine($"  <Source>{Custom1}</Source>");
			sb.AppendLine($"  <SourceGroup>{Custom7}</SourceGroup>");
			sb.AppendLine($"  <Destination>{Custom2}</Destination>");
			sb.AppendLine($"  <DestinationGroup>{Custom8}</DestinationGroup>");
			sb.AppendLine($"  <TimeStamp>{now}</TimeStamp>");
			sb.AppendLine("</InteropSetup>");

			Message = sb.ToString();
		}

		public WorkOrder CreateWorkOrder()
		{
			var chainId = GetRandomId(5);
			var workOrderId = GetRandomId(5);
			var jobNameId = GetRandomId(3);

			return new WorkOrder
			{
				InstanceId = Guid.NewGuid().ToString(),
				WorkOrderId = workOrderId,
				ChainId = chainId,
				StartTime = DateTime.Now.AddHours(4).AddMinutes(1).ToLocalTime(),
				EndTime = DateTime.Now.AddHours(4).AddMinutes(5).ToLocalTime(),
				MessageType = 1, // New
				Custom1 = "Tata-SRT-IP-1",
				Custom2 = "Tata-SRT-OP-1",
				Custom3 = $"Random Name Test {jobNameId}",
				Custom4 = $"CL_ID 10000003",
				Custom5 = "0 Edge Switch",
				Custom6 = "Edge",
				Custom7 = "Tata",
				Custom8 = "Tata",
				Custom9 = -1,
				Custom10 = -1,
				InteropStatus = (int)WorkOrderInteropStatus.Pending,
				UpdateOADate = DateTime.Now.AddHours(4).ToOADate(),
				BookingStatus = (int)WorkOrderStatus.PendingUpdate,
				FailureDescription = (int)WorkOrderStatus.Na,
				//Message goes here
				DisplayKey = $"{chainId}/{workOrderId}",
				PreviousJobName = $"Random Name Test {jobNameId}",
			};
		}

		private static string GetRandomId(int length)
		{
			return string.Join("", Math.Abs(Guid.NewGuid().GetHashCode()).ToString().Take(length));
		}
	}
}
