#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Text;
using Newtonsoft.Json;

namespace Cqrs.EventStore.Bus
{
	/// <summary>
	/// Converts a stream of JSON text using deserialisation.
	/// </summary>
	public class EventConverter
	{
		/// <summary>
		/// Assumes the provided <paramref name="eventData"/> is a strean of JSON text and 
		/// deserialises the provided <paramref name="eventData"/> into an object of type <paramref name="typeName"/> then casts to <typeparamref name="TEvent"/>.
		/// </summary>
		/// <typeparam name="TEvent">The <see cref="Type"/> of the event to convert to.</typeparam>
		/// <param name="eventData">A strean of JSON text</param>
		/// <param name="typeName">The name of the <see cref="Type"/> to deserialise the provided <paramref name="eventData"/> to.</param>
		public static TEvent GetEventFromData<TEvent>(byte[] eventData, string typeName)
		{
			var eventType = Type.GetType(typeName);

			if (eventType == null)
			{
				return default(TEvent);
			}

			string eventjson = Encoding.UTF8.GetString(eventData);
			object eventObject = JsonConvert.DeserializeObject(eventjson, eventType);
			return (TEvent)eventObject;
		}
	}
}