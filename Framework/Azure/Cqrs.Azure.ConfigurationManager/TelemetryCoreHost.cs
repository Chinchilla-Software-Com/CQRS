#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Net;
using Cqrs.Configuration;
using Cqrs.Hosts;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
#if NETSTANDARD2_0
using Microsoft.Extensions.Configuration;
#endif

namespace Cqrs.Azure.ConfigurationManager
{
	/// <summary>
	/// Configure and start command and event handlers in a host with telemetry
	/// </summary>
	public abstract class TelemetryCoreHost<TAuthenticationToken> : CoreHost<TAuthenticationToken>
	{
#if NETSTANDARD2_0
		/// <summary>
		/// Gets or sets the <see cref="IConfigurationManager"/>. This must be set manually as dependency injection may not be ready in-time.
		/// </summary>
		protected static IConfigurationManager _configurationManager = null;

		/// <summary>
		/// Set the <see cref="IConfigurationManager"/> to use before the <see cref="DependencyResolver.Current"/> is set.
		/// Will also set <see cref="DependencyResolver.ConfigurationManager"/>.
		/// </summary>
		/// <param name="configuration"></param>
		public static void SetConfigurationManager(IConfigurationRoot configuration)
		{
			_configurationManager = new CloudConfigurationManager(configuration);
			Configuration.ConfigurationManager.BaseConfiguration = configuration;
			DependencyResolver.ConfigurationManager = _configurationManager;
		}
#endif
#if NET472
		/// <summary>
		/// Gets or sets the <see cref="IConfigurationManager"/>.
		/// </summary>
		protected static readonly IConfigurationManager _configurationManager = new CloudConfigurationManager();
#endif

		/// <summary>
		/// The <see cref="IConfigurationManager"/> that can be use before the <see cref="DependencyResolver.Current"/> is set.
		/// </summary>
		protected override IConfigurationManager ConfigurationManager
		{
			get { return _configurationManager ?? DependencyResolver.ConfigurationManager; }
		}

		/// <summary>
		/// Gets or sets the <see cref="TelemetryClient"/>.
		/// </summary>
		public TelemetryClient TelemetryClient { get; private set; }

#if NETSTANDARD2_0
		/// <summary>
		/// The delegate used internally to get the current <see cref="TelemetryConfiguration"/>.
		/// <see cref="TelemetryConfiguration.CreateDefault"/> will be used if this is not set.
		/// </summary>
		public static Func<TelemetryConfiguration> GetTelemetryConfigurationDelegate { get; set; }
#endif

		#region Overrides of CoreHost<TAuthenticationToken>

		/// <summary>
		/// When overridden, allows you to configure Telemetry
		/// </summary>
		protected override void ConfigureTelemetry()
		{
			string applicationInsightsConnectionString = ConfigurationManager.GetConnectionString("Cqrs.Hosts.ApplicationInsights.ConnectionString") ?? ConfigurationManager.GetSetting("Cqrs.Hosts.ApplicationInsights.ConnectionString");
			if (string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
			{
				string applicationInsightsInstrumentationKey = ConfigurationManager.GetSetting("Cqrs.Hosts.ApplicationInsights.InstrumentationKey");
				if (string.IsNullOrWhiteSpace(applicationInsightsInstrumentationKey))
					applicationInsightsInstrumentationKey = ConfigurationManager.GetSetting("Cqrs.Hosts.ApplicationInsightsInstrumentationKey");
				if (!string.IsNullOrWhiteSpace(applicationInsightsInstrumentationKey))
					applicationInsightsConnectionString = $"InstrumentationKey={applicationInsightsInstrumentationKey}";
			}

			string enabledApplicationInsightsDeveloperModeSetting = ConfigurationManager.GetSetting("Cqrs.Hosts.ApplicationInsights.EnableDeveloperMode");
			if (string.IsNullOrWhiteSpace(enabledApplicationInsightsDeveloperModeSetting))
				enabledApplicationInsightsDeveloperModeSetting = ConfigurationManager.GetSetting("Cqrs.Hosts.EnabledApplicationInsightsDeveloperMode");

#if NETSTANDARD2_0
			TelemetryConfiguration config = GetTelemetryConfigurationDelegate == null
				? TelemetryConfiguration.CreateDefault()
				: GetTelemetryConfigurationDelegate() ?? TelemetryConfiguration.CreateDefault();
			config.ConnectionString = applicationInsightsConnectionString;
#else
			TelemetryConfiguration.Active.ConnectionString = applicationInsightsConnectionString;
#endif
			bool enabledApplicationInsightsDeveloperMode;
			if (!bool.TryParse(enabledApplicationInsightsDeveloperModeSetting, out enabledApplicationInsightsDeveloperMode))
				enabledApplicationInsightsDeveloperMode = false;
#if NETSTANDARD2_0
			config.TelemetryChannel.DeveloperMode = enabledApplicationInsightsDeveloperMode;
			TelemetryClient = new TelemetryClient (config);
#else
			TelemetryConfiguration.Active.TelemetryChannel.DeveloperMode = enabledApplicationInsightsDeveloperMode;
			if (ConfigurationManager.TryGetSetting("Cqrs.Hosts.ApplicationInsights.OperationName", out string operationName) && !string.IsNullOrWhiteSpace(operationName))
				TelemetryConfiguration.Active.TelemetryInitializers.Add(new OperationNameTelemetryInitializer(operationName));
			if (ConfigurationManager.TryGetSetting("Cqrs.Hosts.ApplicationInsights.CloudRoleName", out string cloudRoleName) && !string.IsNullOrWhiteSpace(cloudRoleName))
				TelemetryConfiguration.Active.TelemetryInitializers.Add(new CloudRoleNameTelemetryInitializer(cloudRoleName));
			TelemetryClient = new TelemetryClient(TelemetryConfiguration.Active);
#endif

			TelemetryClient.TrackEvent($"{TelemetryName}/Instantiating");
			TelemetryClient.Flush();
		}

		/// <summary>
		/// Calls <see cref="Prepare"/>, <paramref name="handlerRegistation"/> and then <see cref="Start"/>
		/// </summary>
		public override void Run(Action handlerRegistation = null)
		{
			base.Run(handlerRegistation);
			TelemetryClient.TrackEvent($"{TelemetryName}/Ran");
			TelemetryClient.Flush();
		}

		/// <summary>
		/// Sets the <see cref="System.Net.ServicePointManager.SecurityProtocol"/> to
		/// <see cref="System.Net.SecurityProtocolType.Tls12"/> | <see cref="System.Net.SecurityProtocolType.Tls11"/> | <see cref="System.Net.SecurityProtocolType.Tls"/>.
		/// </summary>
		protected override void PrepareSecurityProtocol()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
		}

		/// <summary>
		/// Prepare the host before registering handlers and starting the host.
		/// </summary>
		protected override void Prepare()
		{
			base.Prepare();

			TelemetryClient.TrackEvent($"{TelemetryName}/Prepared");
		}

		/// <summary>
		/// Start the host post preparing and registering handlers.
		/// </summary>
		protected override void Start()
		{
			base.Start();

			TelemetryClient.TrackEvent($"{TelemetryName}/Started");
		}

		#endregion
	}
}