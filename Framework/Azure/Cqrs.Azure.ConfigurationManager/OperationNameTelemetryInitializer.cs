using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;

namespace Cqrs.Azure.ConfigurationManager
{
	/// <summary>
	/// A <see cref="ITelemetryInitializer"/> that will set the <see cref="OperationContext.Name"/>
	/// </summary>
	public class OperationNameTelemetryInitializer : ITelemetryInitializer
	{
		/// <summary>
		/// The name Operation Name.
		/// </summary>
		public string OperationName { get; protected set; }

		/// <summary>
		/// Instantiates a new instance of the <see cref="OperationNameTelemetryInitializer"/>
		/// </summary>
		/// <param name="operationName">The name Operation Name</param>
		public OperationNameTelemetryInitializer(string operationName)
		{
			OperationName = operationName;
		}

		/// <summary>
		/// Sets the <see cref="OperationContext.Name"/>
		/// </summary>
		/// <param name="telemetry"></param>
		public void Initialize(ITelemetry telemetry)
		{
			// set custom role name here
			telemetry.Context.Operation.Name = OperationName;
		}
	}
}