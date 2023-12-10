#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.IO;
using System.Reflection;
using Chinchilla.Logging;
using Chinchilla.Logging.Configuration;
using Chinchilla.StateManagement;

namespace Cqrs.Configuration
{
	/// <summary>
	/// A collection of extension methods for <see cref="IConfigurationManager"/>.
	/// </summary>
	public static class ConfigurationExtensions
	{
		/// <summary>
		/// Get or Set the <see cref="Func{TResult}"/> that returns the path to the where the execution is occuring.
		/// </summary>
		public static Func<string> GetExecutionPath { get; set; } = () =>
		{
			return AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
		};

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
				string assemblyFile = Path.Combine(GetExecutionPath(), "Chinchilla.Logging.Azure.ApplicationInsights.dll");
				ITelemetryHelper helper = null;
				Action action = () => {
					try
					{
#if NETSTANDARD2_0
						helper = (ITelemetryHelper)DotNetStandard2Helper.CreateInstanceFrom(assemblyFile, "Chinchilla.Logging.Azure.ApplicationInsights.TelemetryHelper", false, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[] { correlationIdHelper, DependencyResolver.Current.Resolve<ILoggerSettings>(), DependencyResolver.Current.Resolve<IContextItemCollectionFactory>(), false }, null, null);
#else
						helper = (ITelemetryHelper)Activator.CreateInstanceFrom(assemblyFile, "Chinchilla.Logging.Azure.ApplicationInsights.TelemetryHelper", false, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[] { correlationIdHelper, DependencyResolver.Current.Resolve<ILoggerSettings>(), DependencyResolver.Current.Resolve<IContextItemCollectionFactory>(), false }, null, null).Unwrap();
#endif
					}
					catch (FileNotFoundException)
					{
						throw;
					}
					catch
					{
#if NETSTANDARD2_0
						helper = (ITelemetryHelper)DotNetStandard2Helper.CreateInstanceFrom(assemblyFile, "Chinchilla.Logging.Azure.ApplicationInsights.TelemetryHelper", false, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[] { correlationIdHelper, null, DependencyResolver.Current.Resolve<IContextItemCollectionFactory>(), false }, null, null);
#else
						helper = (ITelemetryHelper)Activator.CreateInstanceFrom(assemblyFile, "Chinchilla.Logging.Azure.ApplicationInsights.TelemetryHelper", false, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[] { correlationIdHelper, null, DependencyResolver.Current.Resolve<IContextItemCollectionFactory>(), false }, null, null).Unwrap();
#endif
					}
				};
				try
				{
					action();
				}
				catch (FileNotFoundException)
				{
					assemblyFile = Path.Combine(GetExecutionPath(), "..", "Chinchilla.Logging.Azure.ApplicationInsights.dll");
					action();
				}

				if (configurationManager.TryGetSetting("Cqrs.Hosts.ApplicationInsights.CloudRoleName", out string cloudRoleName) && !string.IsNullOrWhiteSpace(cloudRoleName))
				{
					PropertyInfo getCloudRoleNameProperty = helper.GetType().GetProperty("GetCloudRoleName", BindingFlags.Instance | BindingFlags.Public);
					if (getCloudRoleNameProperty != null)
						getCloudRoleNameProperty.SetValue(helper, (Func<string>)(() => { return cloudRoleName; }), null);
				}
				if (configurationManager.TryGetSetting("Cqrs.Hosts.ApplicationInsights.OperationName", out string operationName) && !string.IsNullOrWhiteSpace(operationName))
				{
					PropertyInfo getOperationNameProperty = helper.GetType().GetProperty("GetOperationName", BindingFlags.Instance | BindingFlags.Public);
					if (getOperationNameProperty != null)
						getOperationNameProperty.SetValue(helper, (Func<string>)(() => { return operationName; }), null);
				}

				return helper;
			}
			return new NullTelemetryHelper();
		}
	}
}