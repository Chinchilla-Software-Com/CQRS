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
			{
				var helper = (ITelemetryHelper)Activator.CreateInstanceFrom(string.Format("{0}\\cdmdotnet.Logging.Azure.ApplicationInsights.dll", AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory), "cdmdotnet.Logging.Azure.ApplicationInsights.TelemetryHelper", false, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[] { correlationIdHelper }, null, null).Unwrap();
				return helper;
			}
			return new NullTelemetryHelper();
		}
	}
}