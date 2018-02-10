#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using cdmdotnet.Logging;
using System.Reflection;

namespace Cqrs.Configuration
{
	/// <summary>
	/// A collection of extension methods for <see cref="IConfigurationManager"/>.
	/// </summary>
	public static class ConfigurationExtensions
	{
		/// <summary>
		/// Creates an instance of <see cref="ITelemetryHelper"/> if the value for <paramref name="configurationKey"/> is true.
		/// </summary>
		public static ITelemetryHelper CreateTelemetryHelper(this IConfigurationManager configurationManager, string configurationKey, IDependencyResolver dependencyResolver)
		{
			return CreateTelemetryHelper(configurationManager, configurationKey, dependencyResolver.Resolve<ICorrelationIdHelper>());
		}

		/// <summary>
		/// Creates an instance of <see cref="ITelemetryHelper"/> if the value for <paramref name="configurationKey"/> is true.
		/// </summary>
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