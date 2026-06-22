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
	/// Deserialises <see cref="IEvent{TAuthenticationToken}"/> from a serialised state.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class MongoDbEventDeserialiser<TAuthenticationToken> : EventDeserialiser<TAuthenticationToken>
	{
		#region Overrides of EventDeserialiser<TAuthenticationToken>

		/// <summary>
		/// Deserialise the provided <paramref name="eventData"/> into an <see cref="IEvent{TAuthenticationToken}"/>.
		/// </summary>
		/// <param name="eventData">The <see cref="EventData"/> to Deserialise.</param>
		public override IEvent<TAuthenticationToken> Deserialise(EventData eventData)
		{
			return (IEvent<TAuthenticationToken>)eventData.Data;
		}

		#endregion
	}
}