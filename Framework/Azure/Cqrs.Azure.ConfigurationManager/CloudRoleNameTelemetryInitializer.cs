using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;

namespace Cqrs.Azure.ConfigurationManager
{
	/// <summary>
	/// A <see cref="ITelemetryInitializer"/> that will set the <see cref="CloudContext.RoleName"/>
	/// </summary>
	public class CloudRoleNameTelemetryInitializer : ITelemetryInitializer
	{
		/// <summary>
		/// The name Cloud Role Name.
		/// </summary>
		public string CloudRoleName { get; protected set; }

		/// <summary>
		/// Instantiates a new instance of the <see cref="CloudRoleNameTelemetryInitializer"/>
		/// </summary>
		/// <param name="cloudRoleName">The name Cloud Role Name</param>
		public CloudRoleNameTelemetryInitializer(string cloudRoleName)
		{
			CloudRoleName = cloudRoleName;
		}

		/// <summary>
		/// Sets the <see cref="CloudContext.RoleName"/>
		/// </summary>
		/// <param name="telemetry"></param>
		public void Initialize(ITelemetry telemetry)
		{
			// set custom role name here
			telemetry.Context.Cloud.RoleName = CloudRoleName;
		}
	}
}