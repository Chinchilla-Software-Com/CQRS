using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using EventStore.ClientAPI;

namespace Cqrs.EventStore
{
	public class EventStoreConnectionHelper<TAuthenticationToken> : IEventStoreConnectionHelper
	{
		protected IEventBuilder<TAuthenticationToken> EventBuilder { get; set; }

		public EventStoreConnectionHelper(IEventBuilder<TAuthenticationToken> eventBuilder)
		{
			EventBuilder = eventBuilder;
		}

		public virtual IEventStoreConnection GetEventStoreConnection()
		{
			ConnectionSettings settings = ConnectionSettings.Create();
			IPEndPoint endPoint = GetEventStoreIpEndPoint();
			IEventStoreConnection connection = EventStoreConnection.Create(settings, endPoint);
			connection.ConnectAsync().RunSynchronously();

			EventData connectionEvent = EventBuilder.CreateClientConnectedEvent(GetEventStoreClientName());
			connection.AppendToStreamAsync(GetEventStoreConnectionLogStreamName(), ExpectedVersion.Any, new[] { connectionEvent }).RunSynchronously();

			return connection;
		}

		protected virtual string GetEventStoreClientName()
		{
			return ConfigurationManager.AppSettings["Cqrs.EventStoreClientName"] ?? "Cqrs Default Client";
		}

		protected virtual string GetEventStoreConnectionLogStreamName()
		{
			return ConfigurationManager.AppSettings["Cqrs.EventStoreConnectionLogStreamName"] ?? "EventStore Connection Log Stream";
		}

		protected virtual IPEndPoint GetEventStoreIpEndPoint()
		{
			List<byte> eventStoreIp = (ConfigurationManager.AppSettings["Cqrs.EventStoreIp"] ?? "127.0.0.1").Split(new[] { '.' }).Select(ipPart => (byte)int.Parse(ipPart)).ToList();
			string eventStorePortValue = ConfigurationManager.AppSettings["Cqrs.EventStorePort"];
			int eventStorePort = 1113;
			if (!string.IsNullOrWhiteSpace(eventStorePortValue))
				eventStorePort = int.Parse(eventStorePortValue);
			return new IPEndPoint(new IPAddress(new[] { eventStoreIp[0], eventStoreIp[1], eventStoreIp[2], eventStoreIp[3] }), eventStorePort);
		}
	}
}