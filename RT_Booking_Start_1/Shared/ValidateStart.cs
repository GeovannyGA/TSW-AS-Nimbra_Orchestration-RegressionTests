namespace RT_Booking_Start_1.Shared
{
	using System;
	using System.Threading;
	using Library.HelperMethods;
	using Library.Tests.TestCases;
	using QAPortalAPI.Models.ReportingModels;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;

	public class ValidateStart : ITestCase
	{
		private const int WorkOrderTableId = 1000;
		private const int NumberOfRetries = 3;

		public ValidateStart()
		{
			Name = $"Validate Booking Start: Connection between source and destination in Nimbra Edge element. Along with the booking status changing to in progress.";
		}

		public string Name { get; set; }

		public TestCaseReport TestCaseReport { get; private set; }

		public PerformanceTestCaseReport PerformanceTestCaseReport { get;}

		public void Execute(IEngine engine)
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
	}
}