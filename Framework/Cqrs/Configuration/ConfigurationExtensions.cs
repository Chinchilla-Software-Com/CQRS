using System;
using cdmdotnet.Logging;

namespace Cqrs.Configuration
{
	public static class ConfigurationExtensions
	{
		public static ITelemetryHelper CreateTelemetryHelper(this IConfigurationManager configurationManager, string configurationKey)
		{
			bool useApplicationInsightTelemetryHelper;
			if (!bool.TryParse(configurationManager.GetSetting(configurationKey), out useApplicationInsightTelemetryHelper))
				useApplicationInsightTelemetryHelper = false;

			if (useApplicationInsightTelemetryHelper)
				return (ITelemetryHelper)Activator.CreateInstanceFrom("cdmdotnet.Logging.Azure.ApplicationInsights.dll", "cdmdotnet.Logging.Azure.ApplicationInsights.TelemetryHelper").Unwrap();
			return new NullTelemetryHelper();
		}
	}
}