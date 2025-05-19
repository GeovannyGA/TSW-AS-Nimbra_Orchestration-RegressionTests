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
using static RT_Booking_Start_1.Shared.WorkOrder;

namespace RT_Booking_Start_1.Shared
{
	public class ValidateStart : ITestCase
	{
		private readonly WorkOrder _wordOrder;

		public string Name { get; set; }

		public TestCaseReport TestCaseReport { get; private set; }

		public PerformanceTestCaseReport PerformanceTestCaseReport { get; private set; }

		public ValidateStart(WorkOrder workOrder)
		{
			_wordOrder = workOrder;
		}

		public void Execute(IEngine engine)
		{
			try
			{
				engine.GenerateInformation("@@@@@@@@ TOP OF SCRIPT IN EXECUTE");

				IDms thisDms = engine.GetDms();
				var schedullManager = thisDms.GetElement(new DmsElementId(19803, 67));
				var workOrders = schedullManager.GetTable(1000);
				var workOrder = _wordOrder.CreateWorkOrder();
				workOrder.BuildMessage();
				workOrders.AddRow(workOrder.ToObjectArray());

				Thread.Sleep(60000);

				for (int i = 0; i < 3; i++)
				{
					// Wait 2-3 minutes validate booking status = In Progress
					var recentlyCreatedWO = workOrders.GetRow(workOrder.InstanceId);
					if ((WorkOrderStatus)Convert.ToInt16(recentlyCreatedWO[18]) == WorkOrderStatus.InProgress)
					{
						TestCaseReport = TestCaseReport.GetSuccessTestCase(Name);
						engine.GenerateInformation("@@@@@@@@ The booking has a good status");
						break;
					}
					else
					{
						engine.GenerateInformation("&&&& The booking status is not in progress.");
					}

					Thread.Sleep(2000);
				}

				TestCaseReport = TestCaseReport.GetFailTestCase(Name, "Booking never switched to in progress");
			}
			catch (Exception ex)
			{
				TestCaseReport = TestCaseReport.GetFailTestCase(Name, $"Exception occurred: {ex.Message}");
			}
		}
	}
}
