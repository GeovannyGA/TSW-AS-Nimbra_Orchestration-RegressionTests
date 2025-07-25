﻿namespace RT_Validate_WorkOrder
{
    using Library.HelperMethods;
    using Library.SharedTestCases;
    using Library.Tests.TestCases;
    using QAPortalAPI.Models.ReportingModels;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using System;
    using System.Threading;

    public class ValidateWorkOrder : ITestCase
    {
        private readonly AcknowledgmentParameters _parameters;

        public ValidateWorkOrder(AcknowledgmentParameters parameters)
        {
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));

            Name = $"Validate Work Order: {parameters.JobName} ({parameters.Source} -> {parameters.Destination})";
        }

        public string Name { get; set; }

        public TestCaseReport TestCaseReport { get; private set; }

        public PerformanceTestCaseReport PerformanceTestCaseReport { get; private set; }

        public void Execute(IEngine engine)
        {
            try
            {
                // wait the buffer time for the row to be added
                Thread.Sleep(11000);
                bool isSuccess = IsValidWorkOrder(engine);

                if (isSuccess)
                {
                    TestCaseReport = TestCaseReport.GetSuccessTestCase(Name);
                }
                else
                {
                    TestCaseReport = TestCaseReport.GetFailTestCase(Name, "The work Order was not created correctly");
                }
            }
            catch (Exception ex)
            {
                TestCaseReport = TestCaseReport.GetFailTestCase(Name, $"Exception occurred: {ex.Message}");
            }
        }

        private bool IsValidWorkOrder(IEngine engine)
        {
            RTestIdmsHelper rtestIdmsHelper = new RTestIdmsHelper(engine);
            IDmsElement scheduAll = rtestIdmsHelper.ScheduAllElement;

            if (scheduAll == null)
            {
                return false;
            }

            var table = scheduAll.GetTable(ConstantVariables.TableId);
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

            if (!Convert.ToString(row[ConstantVariables.IndexSource]).Equals(_parameters.Source) || !Convert.ToString(row[ConstantVariables.IndexDestination]).Equals(_parameters.Destination))
            {
                return false;
            }

            if (!Convert.ToString(row[ConstantVariables.IndexJobName]).Equals(_parameters.JobName))
            {
                return false;
            }

            if (!Convert.ToString(row[ConstantVariables.IndexSourceGroup]).Equals(_parameters.SourceGroup) || !Convert.ToString(row[ConstantVariables.IndexDestinationGroup]).Equals(_parameters.DestinationGroup))
            {
                return false;
            }

            if ((WorkOrderStatus)Convert.ToInt16(row[ConstantVariables.IndexStatus]) == WorkOrderStatus.UpdateSent)
            {
                return true;
            }

            var waitTimeParam = scheduAll.GetStandaloneParameter<int?>(ConstantVariables.BufferWaitTime);
            int waitTime = waitTimeParam.GetValue() ?? ConstantVariables.ExceptionValue;

            if (waitTime == ConstantVariables.ExceptionValue)
            {
                return false;
            }

            // Buffer time + 5 seconds in ms
            waitTime = (waitTime + 5) * 1000;

            Thread.Sleep(waitTime);
            object[] rowUpdate = table.GetRow(key);

            if ((WorkOrderStatus)Convert.ToInt16(rowUpdate[ConstantVariables.IndexStatus]) == WorkOrderStatus.UpdateSent || (WorkOrderStatus)Convert.ToInt16(rowUpdate[ConstantVariables.IndexStatus]) == WorkOrderStatus.Created)
            {
                return true;
            }

            return false;
        }

        private string GetRow(IDmsTable table, string[] keys)
        {
            var chainIdColumn = table.GetColumn<string>(ConstantVariables.ChainPid);
            var workOrderIdColumn = table.GetColumn<string>(ConstantVariables.WorkOrderPid);

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
    }
}