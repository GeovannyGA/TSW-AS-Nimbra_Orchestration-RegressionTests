/*
****************************************************************************
*  Copyright (c) 2025,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

dd/mm/2025	1.0.0.1		XXX, Skyline	Initial version
****************************************************************************
*/

namespace RT_Booking_End_1
{
	using System;
	using Library.SharedTestCases;
	using Library.Tests;
	using RT_Booking_Start;
	using RT_Validate_Acknowledgment;
	using RT_Validate_WorkOrder;
	using Skyline.DataMiner.Automation;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		private const string TestName = "RT_Booking_Start";
		private const string TestDescription = "Regression Test to validate the connection and start of a booking.";

		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			try
			{
				// 4 hours added to match the timestamps of scheduAll
				var startTime = DateTime.Now.AddHours(4).AddSeconds(30);
				var endTime = startTime.AddMinutes(5);
				Random random = new Random();

				string randomCircuitId = random.Next(1000000, 10000000).ToString();
				string randomWorkOrderId = random.Next(1000000, 10000000).ToString();

				// Create parameters for the test case
				var parameters = new AcknowledgmentParameters
				{
					Start = startTime,
					End = endTime,
					JobName = "RT Test Booking Start",
					Source = "Tata-SRT-IP-1",
					Destination = "Tata-SRT-OP-1",
					SourceGroup = "Tata",
					DestinationGroup = "Tata",
					Platform = "Test",
					Endpoint = "http://172.16.100.5:8200",
					WorkOrder = randomWorkOrderId,
					ChainId = randomCircuitId,
				};

				Test test = new Test(TestName, TestDescription);
				test.AddTestCase(new ValidateAcknowledgment(parameters));
				test.AddTestCase(new ValidateWorkOrder(parameters));
				test.AddTestCase(new ValidateStart(parameters));

				test.Execute(engine);
				test.PublishResults(engine);
			}
			catch (Exception e)
			{
				engine.Log($"{TestName} failed: {e}");
			}
		}
	}
}