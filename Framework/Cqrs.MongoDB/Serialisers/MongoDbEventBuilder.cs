#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Events;

namespace Cqrs.MongoDB.Serialisers
{
	/// <summary>
	/// Builds <see cref="EventData"/> from various input formats leaving the event data unserialised.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class MongoDbEventBuilder<TAuthenticationToken>
		: DefaultEventBuilder<TAuthenticationToken>
	{
		#region Overrides of EventBuilder<TAuthenticationToken>

		/// <summary>
		/// Create an <see cref="EventData">framework event</see> with the provided <paramref name="eventData"/>.
		/// </summary>
		/// <param name="type">The name of the <see cref="Type"/> of the target object the serialised data is.</param>
		/// <param name="eventData">The <see cref="IEvent{TAuthenticationToken}"/> to add to the <see cref="EventData"/>.</param>
		public override EventData CreateFrameworkEvent(string type, IEvent<TAuthenticationToken> eventData)
		{
			return new EventData
			{
				EventId = Guid.NewGuid(),
				EventType = type,
				Data = eventData
			};
		}

		#endregion
	}
}