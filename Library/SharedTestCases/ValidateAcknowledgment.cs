namespace RT_Validate_Acknowledgment
{
	using System;
	using System.Linq.Expressions;
	using System.Net.Http;
	using System.Net.Sockets;
	using System.Text;
	using System.Threading.Tasks;
	using System.Xml;
	using System.Xml.Linq;

	using Library.Tests.TestCases;

	using QAPortalAPI.Models.ReportingModels;

	using Skyline.DataMiner.Automation;

	public class ValidateAcknowledgment : ITestCase
	{
		private readonly string _xmlRequest;
		private readonly string _endpoint;

		public ValidateAcknowledgment(AcknowledgmentParameters parameters)
		{
			_endpoint = parameters.Endpoint;

			Name = $"Validate Acknowledgment: {parameters.JobName} ({parameters.Source} -> {parameters.Destination})";

			StringBuilder xmlBuilder = new StringBuilder();
			xmlBuilder.Append("<InteropSetup>");
			xmlBuilder.Append("<MessageType>New</MessageType>");
			xmlBuilder.Append("<CircuitID>2756704</CircuitID>");
			xmlBuilder.Append("<SharedID>18922861</SharedID>");
			xmlBuilder.Append("<WorkOrder>1430239</WorkOrder>");
			xmlBuilder.Append("<Client>CL_ID 10000003</Client>");
			xmlBuilder.Append($"<JobName>{parameters.JobName}</JobName>");
			xmlBuilder.Append($"<Start>{parameters.Start.ToString("yyyy/MM/dd HH:mm:ss")}</Start>");
			xmlBuilder.Append($"<End>{parameters.End.ToString("yyyy/MM/dd HH:mm:ss")}</End>");
			xmlBuilder.Append("<ServiceDescription>0 Edge Switch</ServiceDescription>");
			xmlBuilder.Append("<ServiceID>Edge</ServiceID>");
			xmlBuilder.Append($"<Platform>{parameters.Platform}</Platform>");
			xmlBuilder.Append($"<Source>{parameters.Source}</Source>");
			xmlBuilder.Append($"<SourceGroup>{parameters.SourceGroup}</SourceGroup>");
			xmlBuilder.Append($"<Destination>{parameters.Destination}</Destination>");
			xmlBuilder.Append($"<DestinationGroup>{parameters.DestinationGroup}</DestinationGroup>");
			xmlBuilder.Append($"<TimeStamp>{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}</TimeStamp>");
			xmlBuilder.Append("</InteropSetup>");

			// Create the full xmlCmd parameter that will be sent
			_xmlRequest = $"xmlCmd={xmlBuilder.ToString()}";
		}

		public string Name { get; set; }

		public TestCaseReport TestCaseReport { get; private set; }

		public PerformanceTestCaseReport PerformanceTestCaseReport { get; private set; }

		public void Execute(IEngine engine)
		{
			try
			{
				engine.Log("XML->" + _xmlRequest);
				var (isSuccess, responseBody, errorMessage) = SendXmlRequestAsync(_xmlRequest, _endpoint, engine).GetAwaiter().GetResult();

				if (isSuccess)
				{
					bool isValidResponse = ValidateResponseXml(responseBody);

					if (isValidResponse)
					{
						TestCaseReport = TestCaseReport.GetSuccessTestCase(Name);
					}
					else
					{
						TestCaseReport = TestCaseReport.GetFailTestCase(Name, "Response XML format is invalid");
					}
				}
				else
				{
					TestCaseReport = TestCaseReport.GetFailTestCase(Name, $"HTTP Request failed: {errorMessage}");
				}
			}
			catch (Exception ex)
			{
				TestCaseReport = TestCaseReport.GetFailTestCase(Name, $"Exception occurred: {ex.Message}");
			}
		}

		private static void AddElement(XmlDocument doc, XmlElement parent, string name, string value)
		{
			XmlElement element = doc.CreateElement(name);
			element.InnerText = value;
			parent.AppendChild(element);
		}


		private static async Task<(bool isSuccess, string responseBody, string errorMessage)> SendXmlRequestAsync(string xmlCmd, string endpoint, IEngine engine)
		{
			try
			{
				using (var httpClient = new HttpClient())
				{
					httpClient.Timeout = TimeSpan.FromSeconds(30);
					var content = new StringContent(xmlCmd, Encoding.UTF8, "text/plain");
					content.Headers.ContentType.CharSet = null;

					engine.Log($"Sending request to {endpoint}");
					engine.Log($"Payload: {xmlCmd}");

					var response = await httpClient.PostAsync(endpoint, content);

					string responseBody = await response.Content.ReadAsStringAsync();

					engine.Log($"Received status code: {response.StatusCode}");
					engine.Log("Response body:");
					engine.Log(responseBody);

					bool isSuccess = response.IsSuccessStatusCode;
					return (isSuccess, responseBody, isSuccess ? null : $"Status code: {response.StatusCode}");
				}
			}
			catch (Exception ex)
			{
				engine.Log($"HTTP request failed: {ex.Message}");
				return (false, null, ex.Message);
			}
		}

		private static bool ValidateResponseXml(string xml)
		{
			try
			{
				// Parse the XML
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(xml);

				// Validate structure - check if it matches expected response format
				XmlNode interopSetup = doc.SelectSingleNode("/InteropSetup");
				if (interopSetup == null)
				{
					return false;
				}

				XmlNode response = interopSetup.SelectSingleNode("Response");
				if (response == null)
				{
					return false;
				}

				XmlNode circuitId = response.SelectSingleNode("CircuitID");
				if (circuitId == null)
				{
					return false;
				}

				XmlNode messageType = response.SelectSingleNode("MessageType");
				if (messageType == null || messageType.InnerText != "New")
				{
					return false;
				}

				XmlNode statusCode = response.SelectSingleNode("StatusCode");
				if (statusCode == null || statusCode.InnerText != "200")
				{
					return false;
				}

				// All validations passed
				return true;
			}
			catch (Exception)
			{
				// XML parsing failed
				return false;
			}
		}

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
		}
	}
}
