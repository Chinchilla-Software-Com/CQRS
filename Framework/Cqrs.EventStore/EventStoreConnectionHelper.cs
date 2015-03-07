using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Cqrs.Configuration;
using EventStore.ClientAPI;

namespace Cqrs.EventStore
{
	public class EventStoreConnectionHelper<TAuthenticationToken> : IEventStoreConnectionHelper
	{
		protected IEventBuilder<TAuthenticationToken> EventBuilder { get; private set; }

		protected IConfigurationManager ConfigurationManager { get; private set; }

		public EventStoreConnectionHelper(IEventBuilder<TAuthenticationToken> eventBuilder, IConfigurationManager configurationManager)
		{
			EventBuilder = eventBuilder;
			ConfigurationManager = configurationManager;
		}

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

		protected virtual string GetEventStoreClientName()
		{
			return ConfigurationManager.GetSetting("Cqrs.EventStoreClientName") ?? "Cqrs Default Client";
		}

		protected virtual string GetEventStoreConnectionLogStreamName()
		{
			return ConfigurationManager.GetSetting("Cqrs.EventStoreConnectionLogStreamName") ?? "EventStore Connection Log Stream";
		}

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