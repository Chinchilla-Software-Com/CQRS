#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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
			DefaultSettings = new JsonSerializerSettings
			{
				DateFormatHandling = DateFormatHandling.IsoDateFormat,
				DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
				DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
				FloatFormatHandling = FloatFormatHandling.DefaultValue,
				NullValueHandling = NullValueHandling.Include,
				PreserveReferencesHandling = PreserveReferencesHandling.All,
				ReferenceLoopHandling = ReferenceLoopHandling.Error,
				StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
				TypeNameHandling = TypeNameHandling.All
			};
		}

		public string SerialiseEvent<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			return JsonConvert.SerializeObject(@event, DefaultSettings);
		}

		public string SerialiseCommand<TCommand>(TCommand command) where TCommand : ICommand<TAuthenticationToken>
		{
			return JsonConvert.SerializeObject(command, DefaultSettings);
		}

		public IEvent<TAuthenticationToken> DeserialiseEvent(string @event)
		{
			return JsonConvert.DeserializeObject<IEvent<TAuthenticationToken>>(@event, DefaultSettings);
		}

		public ICommand<TAuthenticationToken> DeserialiseCommand(string @event)
		{
			return JsonConvert.DeserializeObject<ICommand<TAuthenticationToken>>(@event, DefaultSettings);
		}
	}
}