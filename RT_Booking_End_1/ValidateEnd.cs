namespace RT_Booking_End
{
	using System;
	using System.IO;
	using System.Linq;
	using Library.HelperMethods;
	using Library.SharedTestCases;
	using Library.Tests.TestCases;
	using QAPortalAPI.Models.ReportingModels;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Library.Solutions.SRM.Model.ReservationAction;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.Sections;

	public class ValidateEnd : ITestCase
	{
		public string Name { get; set; }

		public TestCaseReport TestCaseReport { get; private set; }

		public PerformanceTestCaseReport PerformanceTestCaseReport { get; }

		private readonly AcknowledgmentParameters _parameters;

		public ValidateEnd(AcknowledgmentParameters parameters)
		{
			_parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
			Name = $"Validate Booking Start: Connection between source and destination in Nimbra Edge element. Along with the booking status changing to in progress.";
		}

		public void Execute(IEngine engine)
		{
			try
			{
				if (IsBookingSetToEnd(engine))
				{
					TestCaseReport = TestCaseReport.GetSuccessTestCase(Name);
				}

				TestCaseReport = TestCaseReport.GetFailTestCase(Name, "The ending of a booking failed.");

			}
			catch (Exception ex)
			{
				TestCaseReport = TestCaseReport.GetFailTestCase(Name, $"Exception occurred: {ex.Message}");
			}
		}

		private bool IsBookingSetToEnd(IEngine engine)
		{
			Element bookingElement = engine.FindElement(ConstantVariables.BookingManagerElement);
			BookingManager bookingManager = new BookingManager((Engine)engine, bookingElement, true);

			var bookingName = GetBookingName(_parameters.ChainId, _parameters.WorkOrder, _parameters.JobName);
			var filterName = ReservationInstanceExposers.Name.Equal(bookingName);
			var currentReservation = SrmManagers.ResourceManager.GetReservationInstances(filterName)[0];

			if (currentReservation == null)
			{
				return false;
			}

			var changeTimeInputData = new ChangeTimeInputData
			{
				EndDate = DateTime.Now.AddHours(4).AddSeconds(30),
				StartDate = currentReservation.Start.ToLocalTime(),
				IsSilent = true,
				PreRoll = new TimeSpan(0, 0, 0),
				PostRoll = new TimeSpan(0, 0, 0),
			};

			if (bookingManager.TryChangeTime(engine as Engine, ref currentReservation, changeTimeInputData))
			{
				return true;
			}

			return false;
		}

		private static string GetBookingName(string circuitId, string workOrder, string jobName)
		{
			string bookingname;

			if (String.IsNullOrEmpty(jobName))
			{
				bookingname = $"{workOrder}-{circuitId}";
			}
			else
			{
				bookingname = $"{workOrder}-{circuitId}-{jobName}";
			}

			char[] invalidChars = Path.GetInvalidFileNameChars();
			bookingname = String.Join(String.Empty, bookingname.Select(c => invalidChars.Contains(c) ? ' ' : c));
			return bookingname;
		}
	}
}
