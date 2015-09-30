#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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