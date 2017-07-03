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
	public class MongoDbEventBuilder<TAuthenticationToken>
		: DefaultEventBuilder<TAuthenticationToken>
	{
		#region Overrides of EventBuilder<TAuthenticationToken>

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