using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Cqrs.EventStore
{
	public abstract class ProjectionReader<TAuthenticationToken>
	{
		protected IEventStoreConnectionHelper EventStoreConnectionHelper { get; set; }

		protected IEventDeserialiser<TAuthenticationToken> EventDeserialiser { get; set; }

		protected ProjectionReader(IEventStoreConnectionHelper eventStoreConnectionHelper, IEventDeserialiser<TAuthenticationToken> eventDeserialiser)
		{
			EventStoreConnectionHelper = eventStoreConnectionHelper;
			EventDeserialiser = eventDeserialiser;
		}

		protected IEnumerable<dynamic> GetDataByStreamName(string streamName)
		{
			StreamEventsSlice eventCollection;
			using (IEventStoreConnection connection = EventStoreConnectionHelper.GetEventStoreConnection())
			{
				eventCollection = connection.ReadStreamEventsBackwardAsync(streamName, StreamPosition.End, 1, false).Result;
			}
			var jsonSerialiserSettings = EventDeserialiser.GetSerialisationSettings();
			var encoder = new UTF8Encoding();
			return
			(
				(
					(IEnumerable<dynamic>)eventCollection.Events
					.Select(e => JsonConvert.DeserializeObject(((dynamic)encoder.GetString(e.Event.Data)), jsonSerialiserSettings))
					.SingleOrDefault()
				)
					??
				(
					Enumerable.Empty<dynamic>()
				)
			)
			.Select(x => x.Value);
		}

		protected IEnumerable<TData> GetDataByStreamName<TData>(string streamName)
		{
			IList<TData> data = GetDataByStreamName(streamName)
				.Select(e => JsonConvert.DeserializeObject<TData>(e.ToString()))
				.Cast<TData>()
				.ToList();
			return data;
		}
	}
}