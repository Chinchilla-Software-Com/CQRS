#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Chinchilla.Logging;
using System.Reflection;
using Chinchilla.Logging.Configuration;

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
				ITelemetryHelper helper;
				try
				{
					helper = (ITelemetryHelper)Activator.CreateInstanceFrom(string.Format("{0}\\Chinchilla.Logging.Azure.ApplicationInsights.dll", AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory), "Chinchilla.Logging.Azure.ApplicationInsights.TelemetryHelper", false, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[] { correlationIdHelper, DependencyResolver.Current.Resolve<ILoggerSettings>() }, null, null).Unwrap();
				}
				catch
				{
					helper = (ITelemetryHelper)Activator.CreateInstanceFrom(string.Format("{0}\\Chinchilla.Logging.Azure.ApplicationInsights.dll", AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory), "Chinchilla.Logging.Azure.ApplicationInsights.TelemetryHelper", false, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[] { correlationIdHelper }, null, null).Unwrap();
				}
				return helper;
			}
			return new NullTelemetryHelper();
		}
	}
}