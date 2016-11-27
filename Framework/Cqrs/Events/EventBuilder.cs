#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Text;

namespace Cqrs.Events
{
	public abstract class EventBuilder<TAuthenticationToken> : IEventBuilder<TAuthenticationToken>
	{
		#region Implementation of IEventBuilder

		public virtual EventData CreateFrameworkEvent(string type, IEvent<TAuthenticationToken> eventData)
		{
			return new EventData
			{
				EventId = Guid.NewGuid(),
				EventType = type,
				Data = SerialiseEventDataToString(eventData)
			};
		}

		public virtual EventData CreateFrameworkEvent(IEvent<TAuthenticationToken> eventData)
		{
			return CreateFrameworkEvent(eventData.GetType().AssemblyQualifiedName, eventData);
		}

		#endregion

		protected virtual byte[] SerialiseEventData(IEvent<TAuthenticationToken> eventData)
		{
			return new UTF8Encoding().GetBytes(SerialiseEventDataToString(eventData));
		}

		protected abstract string SerialiseEventDataToString(IEvent<TAuthenticationToken> eventData);
	}
}