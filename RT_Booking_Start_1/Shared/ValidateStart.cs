namespace RT_Booking_Start_1.Shared
{
	using System;
	using System.Threading;
	using Library.HelperMethods;
	using Library.SharedTestCases;
	using Library.Tests.TestCases;
	using QAPortalAPI.Models.ReportingModels;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;

	public class ValidateStart : ITestCase
	{
		private const int WorkOrderTableId = 1000;
		private const int NumberOfRetries = 5;
		private const int TableId = 1000;
		private const int ChainPid = 1003;
		private const int WorkOrderPid = 1002;
		private readonly AcknowledgmentParameters _parameters;

		public ValidateStart(AcknowledgmentParameters parameters)
		{
			_parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
			Name = $"Validate Booking Start: Connection between source and destination in Nimbra Edge element. Along with the booking status changing to in progress.";
		}

		public string Name { get; set; }

		public TestCaseReport TestCaseReport { get; private set; }

		public PerformanceTestCaseReport PerformanceTestCaseReport { get;}

		public void Execute(IEngine engine)
		{
			try
			{
				// wait the buffer time for the row to be added
				Thread.Sleep(60000);
				bool isSuccess = IsBookingStatusSetToInProgress(engine);

				if (isSuccess)
				{
					TestCaseReport = TestCaseReport.GetSuccessTestCase(Name);
				}
				else
				{
					TestCaseReport = TestCaseReport.GetFailTestCase(Name, "The booking was not set to in progress within the time defined.");
				}
			}
			catch (Exception ex)
			{
				TestCaseReport = TestCaseReport.GetFailTestCase(Name, $"Exception occurred: {ex.Message}");
			}
		}

		private bool IsBookingStatusSetToInProgress(IEngine engine)
		{
			RTestIdmsHelper rtestIdmsHelper = new RTestIdmsHelper(engine);
			IDmsElement scheduAll = rtestIdmsHelper.ScheduAllElement;
			var table = scheduAll.GetTable(TableId);
			string[] keys = table.GetPrimaryKeys();

			if (keys.Length == 0)
			{
				return false;
			}

			string key = GetRow(table, keys);

			if (String.IsNullOrEmpty(key))
			{
				return false;
			}

			object[] row = table.GetRow(key);

			if ((WorkOrderStatus)Convert.ToInt16(row[18]) == WorkOrderStatus.InProgress)
			{
				return true;
			}

			return false;
		}

		public void ExecuteOld(IEngine engine)
		{
			try
			{
				RTestIdmsHelper rtestIdmsHelper = new RTestIdmsHelper(engine);
				IDmsElement scheduAll = rtestIdmsHelper.ScheduAllElement;
				var workOrders = scheduAll.GetTable(WorkOrderTableId);
				WorkOrder workOrderEmpty = new WorkOrder();
				var workOrder = workOrderEmpty.CreateWorkOrder();
				workOrder.BuildMessage();
				workOrders.AddRow(workOrder.ToObjectArray());

				// Wait 70 seconds after booking creation to start
				Thread.Sleep(70000);

				for (int i = 0; i < NumberOfRetries; i++)
				{
					var recentlyCreatedWO = workOrders.GetRow(workOrder.InstanceId);

					if ((WorkOrderStatus)Convert.ToInt16(recentlyCreatedWO[18]) == WorkOrderStatus.InProgress)
					{
						TestCaseReport = TestCaseReport.GetSuccessTestCase(Name);

						return;
					}

					// Wait 10 seconds between each retry
					Thread.Sleep(10000);
				}

				TestCaseReport = TestCaseReport.GetFailTestCase(Name, "Booking never switched to in progress.");
			}
			catch (Exception ex)
			{
				TestCaseReport = TestCaseReport.GetFailTestCase(Name, $"Exception occurred: {ex.Message}");
			}
		}

		private string GetRow(IDmsTable table, string[] keys)
		{
			var chainIdColumn = table.GetColumn<string>(ChainPid);
			var workOrderIdColumn = table.GetColumn<string>(WorkOrderPid);

			for (int i = 0; i < keys.Length; i++)
			{
				string chainId = chainIdColumn.GetValue(keys[i], KeyType.PrimaryKey);
				string workOrderId = workOrderIdColumn.GetValue(keys[i], KeyType.PrimaryKey);
				if (chainId.Equals(_parameters.ChainId) && workOrderId.Equals(_parameters.WorkOrder))
				{
					return keys[i];
				}
			}

			return String.Empty;
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
}