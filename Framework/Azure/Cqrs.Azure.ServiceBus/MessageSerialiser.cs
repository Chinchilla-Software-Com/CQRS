#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Commands;
using Cqrs.Events;
using Newtonsoft.Json;

namespace Cqrs.Azure.ServiceBus
{
	public class MessageSerialiser<TAuthenticationToken> : IMessageSerialiser<TAuthenticationToken>
	{
		public static JsonSerializerSettings DefaultSettings { get; private set; }

		static MessageSerialiser()
		{
			DefaultSettings = DefaultJsonSerializerSettings.DefaultSettings;
		}

		public string SerialiseEvent<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			return JsonConvert.SerializeObject(@event, GetSerialisationSettings());
		}

		public string SerialiseCommand<TCommand>(TCommand command) where TCommand : ICommand<TAuthenticationToken>
		{
			return JsonConvert.SerializeObject(command, GetSerialisationSettings());
		}

		public IEvent<TAuthenticationToken> DeserialiseEvent(string @event)
		{
			return JsonConvert.DeserializeObject<IEvent<TAuthenticationToken>>(@event, GetSerialisationSettings());
		}

		public ICommand<TAuthenticationToken> DeserialiseCommand(string @event)
		{
			return JsonConvert.DeserializeObject<ICommand<TAuthenticationToken>>(@event, GetSerialisationSettings());
		}

		protected virtual JsonSerializerSettings GetSerialisationSettings()
		{
			return DefaultSettings;
		}
	}
}