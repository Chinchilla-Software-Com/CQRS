#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Events;

namespace Cqrs.MongoDB.Serialisers
{
	public class MongoEventDeserialiser<TAuthenticationToken> : EventDeserialiser<TAuthenticationToken>
	{
		#region Overrides of EventDeserialiser<TAuthenticationToken>

		public override IEvent<TAuthenticationToken> Deserialise(EventData eventData)
		{
			return (IEvent<TAuthenticationToken>)eventData.Data;
		}

		#endregion
	}
}