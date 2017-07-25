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
using System.Text;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Cqrs.EventStore
{
	/// <summary>
	/// Reads projection streams from a Greg Young's Event sTore.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public abstract class ProjectionReader<TAuthenticationToken>
	{
		/// <summary>
		/// The <see cref="IEventStoreConnection"/> used to read and write streams in the Greg Young Event Store.
		/// </summary>
		protected IEventStoreConnectionHelper EventStoreConnectionHelper { get; set; }

		/// <summary>
		/// The <see cref="IEventDeserialiser{TAuthenticationToken}"/> used to deserialise events.
		/// </summary>
		protected IEventDeserialiser<TAuthenticationToken> EventDeserialiser { get; set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="ProjectionReader{TAuthenticationToken}"/>.
		/// </summary>
		protected ProjectionReader(IEventStoreConnectionHelper eventStoreConnectionHelper, IEventDeserialiser<TAuthenticationToken> eventDeserialiser)
		{
			EventStoreConnectionHelper = eventStoreConnectionHelper;
			EventDeserialiser = eventDeserialiser;
		}

		/// <summary>
		/// Get a collection of data objects from a stream with the provided <paramref name="streamName"/>.
		/// </summary>
		/// <param name="streamName">The name of the stream to read events from.</param>
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

		/// <summary>
		/// Get a collection of <typeparamref name="TData"/> from a stream with the provided <paramref name="streamName"/>.
		/// </summary>
		/// <param name="streamName">The name of the stream to read events from.</param>
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