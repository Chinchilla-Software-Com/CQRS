#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Events;

namespace Cqrs.MongoDB.Serialisers
{
	public class MongoDbEventDeserialiser<TAuthenticationToken> : EventDeserialiser<TAuthenticationToken>
	{
		#region Overrides of EventDeserialiser<TAuthenticationToken>

		public override IEvent<TAuthenticationToken> Deserialise(EventData eventData)
		{
			return (IEvent<TAuthenticationToken>)eventData.Data;
		}

		#endregion
	}
}