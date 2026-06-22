#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Text;

namespace Cqrs.Events
{
	/// <summary>
	/// Builds <see cref="EventData"/> from various input formats.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public abstract class EventBuilder<TAuthenticationToken> : IEventBuilder<TAuthenticationToken>
	{
		#region Implementation of IEventBuilder<TAuthenticationToken>

		/// <summary>
		/// Create an <see cref="EventData">framework event</see> with the provided <paramref name="eventData"/>.
		/// </summary>
		/// <param name="type">The name of the <see cref="Type"/> of the target object the serialised data is.</param>
		/// <param name="eventData">The <see cref="IEvent{TAuthenticationToken}"/> to add to the <see cref="EventData"/>.</param>
		public virtual EventData CreateFrameworkEvent(string type, IEvent<TAuthenticationToken> eventData)
		{
			return new EventData
			{
				EventId = Guid.NewGuid(),
				EventType = type,
				Data = SerialiseEventDataToString(eventData)
			};
		}

		/// <summary>
		/// Create an <see cref="EventData">framework event</see> with the provided <paramref name="eventData"/>.
		/// </summary>
		/// <param name="eventData">The <see cref="IEvent{TAuthenticationToken}"/> to add to the <see cref="EventData"/>.</param>
		public virtual EventData CreateFrameworkEvent(IEvent<TAuthenticationToken> eventData)
		{
			return CreateFrameworkEvent(eventData.GetType().AssemblyQualifiedName, eventData);
		}

		#endregion

		/// <summary>
		/// Serialise the provided <paramref name="eventData"/> into a <see cref="byte"/> <see cref="Array"/>.
		/// </summary>
		/// <param name="eventData">The <see cref="IEvent{TAuthenticationToken}"/> to serialise.</param>
		protected virtual byte[] SerialiseEventData(IEvent<TAuthenticationToken> eventData)
		{
			return new UTF8Encoding().GetBytes(SerialiseEventDataToString(eventData));
		}

		/// <summary>
		/// Serialise the provided <paramref name="eventData"/> into a <see cref="string"/>.
		/// </summary>
		/// <param name="eventData">The <see cref="IEvent{TAuthenticationToken}"/> to serialise.</param>
		protected abstract string SerialiseEventDataToString(IEvent<TAuthenticationToken> eventData);
	}
}