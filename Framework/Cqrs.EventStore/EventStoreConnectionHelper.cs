#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Cqrs.Configuration;
using EventStore.ClientAPI;

namespace Cqrs.EventStore
{
	/// <summary>
	/// Creates instances of <see cref="IEventStoreConnection"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class EventStoreConnectionHelper<TAuthenticationToken> : IEventStoreConnectionHelper
	{
		/// <summary>
		/// The <see cref="IEventBuilder{TAuthenticationToken}"/> that is used.
		/// </summary>
		protected IEventBuilder<TAuthenticationToken> EventBuilder { get; private set; }

		/// <summary>
		/// The <see cref="IConfigurationManager"/> that is used.
		/// </summary>
		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="EventStoreConnectionHelper{TAuthenticationToken}"/>
		/// </summary>
		/// <param name="eventBuilder">The <see cref="IEventBuilder{TAuthenticationToken}"/> that is used.</param>
		/// <param name="configurationManager">The <see cref="IConfigurationManager"/> that is used.</param>
		public EventStoreConnectionHelper(IEventBuilder<TAuthenticationToken> eventBuilder, IConfigurationManager configurationManager)
		{
			EventBuilder = eventBuilder;
			ConfigurationManager = configurationManager;
		}

		/// <summary>
		/// Gets a <see cref="IEventStoreConnection"/>
		/// </summary>
		public virtual IEventStoreConnection GetEventStoreConnection()
		{
			ConnectionSettings settings = ConnectionSettings.Create();
			IPEndPoint endPoint = GetEventStoreIpEndPoint();
			IEventStoreConnection connection = EventStoreConnection.Create(settings, endPoint);
			Task connecting = connection.ConnectAsync();
			connecting.Wait();

			EventData connectionEvent = EventBuilder.CreateClientConnectedEvent(GetEventStoreClientName());
			Task notify = connection.AppendToStreamAsync(GetEventStoreConnectionLogStreamName(), ExpectedVersion.Any, connectionEvent);
			notify.Wait();

			return connection;
		}

		/// <summary>
		/// Get the client name from the <see cref="ConfigurationManager"/> that describes the client that will connect to the server.
		/// </summary>
		protected virtual string GetEventStoreClientName()
		{
			return ConfigurationManager.GetSetting("Cqrs.EventStoreClientName") ?? "Cqrs Default Client";
		}

		/// <summary>
		/// Get the connection stream name from the <see cref="ConfigurationManager"/>.
		/// </summary>
		protected virtual string GetEventStoreConnectionLogStreamName()
		{
			return ConfigurationManager.GetSetting("Cqrs.EventStoreConnectionLogStreamName") ?? "EventStore Connection Log Stream";
		}

		/// <summary>
		/// Get the IP address of the server from the <see cref="ConfigurationManager"/>.
		/// </summary>
		protected virtual IPEndPoint GetEventStoreIpEndPoint()
		{
			List<byte> eventStoreIp = (ConfigurationManager.GetSetting("Cqrs.EventStoreIp") ?? "127.0.0.1").Split('.').Select(ipPart => (byte)int.Parse(ipPart)).ToList();
			string eventStorePortValue = ConfigurationManager.GetSetting("Cqrs.EventStorePort");
			int eventStorePort = 1113;
			if (!string.IsNullOrWhiteSpace(eventStorePortValue))
				eventStorePort = int.Parse(eventStorePortValue);
			return new IPEndPoint(new IPAddress(new[] { eventStoreIp[0], eventStoreIp[1], eventStoreIp[2], eventStoreIp[3] }), eventStorePort);
		}
	}
}