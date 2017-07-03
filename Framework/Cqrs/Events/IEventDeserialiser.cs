#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Events
{
	public interface IEventDeserialiser<TAuthenticationToken>
	{
		IEvent<TAuthenticationToken> Deserialise(EventData eventData);
	}
}