using System;
using cdmdotnet.Logging;
using System.Reflection;

namespace Cqrs.Configuration
{
	public static class ConfigurationExtensions
	{
		public static ITelemetryHelper CreateTelemetryHelper(this IConfigurationManager configurationManager, string configurationKey, IDependencyResolver dependencyResolver)
		{
			return CreateTelemetryHelper(configurationManager, configurationKey, dependencyResolver.Resolve<ICorrelationIdHelper>());
		}

		public static ITelemetryHelper CreateTelemetryHelper(this IConfigurationManager configurationManager, string configurationKey, ICorrelationIdHelper correlationIdHelper)
		{
			bool useApplicationInsightTelemetryHelper;
			if (!bool.TryParse(configurationManager.GetSetting(configurationKey), out useApplicationInsightTelemetryHelper))
				useApplicationInsightTelemetryHelper = false;

			if (useApplicationInsightTelemetryHelper)
				return (ITelemetryHelper)Activator.CreateInstanceFrom("cdmdotnet.Logging.Azure.ApplicationInsights.dll", "cdmdotnet.Logging.Azure.ApplicationInsights.TelemetryHelper", false, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[] { correlationIdHelper }, null, null).Unwrap();
			return new NullTelemetryHelper();
		}
	}
}