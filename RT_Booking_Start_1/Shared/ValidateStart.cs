using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Library.Tests.TestCases;
using QAPortalAPI.Models.ReportingModels;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Core.DataMinerSystem.Automation;
using Skyline.DataMiner.Core.DataMinerSystem.Common;
using Skyline.DataMiner.Net.ReportsAndDashboards;
using static RT_Booking_Start_1.Shared.WorkOrder;

namespace RT_Booking_Start_1.Shared
{
	public class ValidateStart : ITestCase
	{
		public string Name { get; set; }

		public TestCaseReport TestCaseReport { get; private set; }

		public PerformanceTestCaseReport PerformanceTestCaseReport { get; private set; }

		public ValidateStart()
		{
			Name = $"Validate Booking Start: Connection between source and destination in Nimbra Edge element. Along with the booking status changing to in progress.";
		}

		public void Execute(IEngine engine)
		{
			try
			{
				IDms thisDms = engine.GetDms();
				var schedullManager = thisDms.GetElement(new DmsElementId(19803, 67));
				var workOrders = schedullManager.GetTable(1000);
				WorkOrder workOrderEmpty = new WorkOrder();
				var workOrder = workOrderEmpty.CreateWorkOrder();
				workOrder.BuildMessage();
				workOrders.AddRow(workOrder.ToObjectArray());

				Thread.Sleep(70000);

				for (int i = 0; i < 3; i++)
				{
					// Wait 2-3 minutes validate booking status = In Progress
					var recentlyCreatedWO = workOrders.GetRow(workOrder.InstanceId);

					if ((WorkOrderStatus)Convert.ToInt16(recentlyCreatedWO[18]) == WorkOrderStatus.InProgress)
					{
						TestCaseReport = TestCaseReport.GetSuccessTestCase(Name);
						return;
					}

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
