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

namespace Cqrs.Azure.ConfigurationManager
{
	/// <summary>
	/// Configure and start command and event handlers in a host with telemetry
	/// </summary>
	public abstract class TelemetryCoreHost<TAuthenticationToken> : CoreHost<TAuthenticationToken>
	{
		protected static readonly IConfigurationManager _configurationManager = new CloudConfigurationManager();

		/// <summary>
		/// The <see cref="IConfigurationManager"/> that can be use before the <see cref="DependencyResolver.Current"/> is set.
		/// </summary>
		protected override IConfigurationManager ConfigurationManager
		{
			get { return _configurationManager; }
		}

		public TelemetryClient TelemetryClient { get; private set; }

		#region Overrides of CoreHost<TAuthenticationToken>

		/// <summary>
		/// When overridden, allows you to configure Telemetry
		/// </summary>
		protected override void ConfigureTelemetry()
		{
			TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.GetSetting("Cqrs.Hosts.ApplicationInsightsInstrumentationKey");
			bool enabledApplicationInsightsDeveloperMode;
			if (!bool.TryParse(ConfigurationManager.GetSetting("Cqrs.Hosts.EnabledApplicationInsightsDeveloperMode"), out enabledApplicationInsightsDeveloperMode))
				enabledApplicationInsightsDeveloperMode = false;
			TelemetryConfiguration.Active.TelemetryChannel.DeveloperMode = enabledApplicationInsightsDeveloperMode;

			TelemetryClient = new TelemetryClient {InstrumentationKey = TelemetryConfiguration.Active.InstrumentationKey};
			TelemetryClient.TrackEvent(string.Format("{0}/Instantiating", TelemetryName));
			TelemetryClient.Flush();
		}

		/// <summary>
		/// Calls <see cref="Prepare"/>, <paramref name="handlerRegistation"/> and then <see cref="Start"/>
		/// </summary>
		public override void Run(Action handlerRegistation = null)
		{
			base.Run(handlerRegistation);
			TelemetryClient.TrackEvent(string.Format("{0}/Ran", TelemetryName));
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

			TelemetryClient.TrackEvent(string.Format("{0}/Prepared", TelemetryName));
		}

		/// <summary>
		/// Start the host post preparing and registering handlers.
		/// </summary>
		protected override void Start()
		{
			base.Start();

			TelemetryClient.TrackEvent(string.Format("{0}/Started", TelemetryName));
		}

		#endregion
	}
}