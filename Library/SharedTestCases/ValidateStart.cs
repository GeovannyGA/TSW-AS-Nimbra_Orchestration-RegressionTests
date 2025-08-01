﻿namespace RT_Booking_Start
{
    using System;
    using System.Threading;
    using Library.HelperMethods;
    using Library.SharedTestCases;
    using Library.Tests.TestCases;
    using QAPortalAPI.Models.ReportingModels;
    using Skyline.DataMiner.Automation;

    public class ValidateStart : ITestCase
    {
        private const int _numberOfRetries = 3;
        private readonly AcknowledgmentParameters _parameters;

        public ValidateStart(AcknowledgmentParameters parameters)
        {
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            Name = $"Validate Booking Start: Connection between source and destination in Nimbra Edge element. Along with the booking status changing to in progress.";
        }

        public string Name { get; set; }

        public TestCaseReport TestCaseReport { get; private set; }

        public PerformanceTestCaseReport PerformanceTestCaseReport { get; }

        public void Execute(IEngine engine)
        {
            try
            {
                // wait the buffer time for the row to be added
                Thread.Sleep(60000);

                bool isSuccess = false;

                for (int i = 1; i <= _numberOfRetries; i++)
                {
                    isSuccess = IsBookingStatusSetToInProgress(engine);
                    if (isSuccess)
                    {
                        break;
                    }

                    Thread.Sleep(5000);
                }

                TestCaseReport = isSuccess ? TestCaseReport.GetSuccessTestCase(Name) : TestCaseReport.GetFailTestCase(Name, "The booking was not set to in progress within the time defined.");
            }
            catch (Exception ex)
            {
                TestCaseReport = TestCaseReport.GetFailTestCase(Name, $"Exception occurred: {ex.Message}");
            }
        }

        private bool IsBookingStatusSetToInProgress(IEngine engine)
        {
            RTestIdmsHelper rtestIdmsHelper = new RTestIdmsHelper(engine);

            if (rtestIdmsHelper.ScheduAllElement == null)
            {
                throw new InvalidOperationException("ScheduAllElement was null or not found");
            }

            var table = rtestIdmsHelper.ScheduAllElement.GetTable(ConstantVariables.TableId);
            string key = RTestIdmsHelper.GetRowWithChainIdAndWorkOrderId(table, _parameters.ChainId, _parameters.WorkOrder);

            if (String.IsNullOrEmpty(key))
            {
                return false;
            }

            object[] row = table.GetRow(key);

            if ((WorkOrderStatus)Convert.ToInt16(row[ConstantVariables.IndexStatus]) == WorkOrderStatus.InProgress)
            {
                return true;
            }

            return false;
        }
    }
}