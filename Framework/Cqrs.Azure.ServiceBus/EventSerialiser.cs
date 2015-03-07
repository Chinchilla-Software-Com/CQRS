#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Events;
using Newtonsoft.Json;

namespace Cqrs.Azure.ServiceBus
{
	public class EventSerialiser<TAuthenticationToken> : IEventSerialiser<TAuthenticationToken>
	{
		public static JsonSerializerSettings DefaultSettings { get; private set; }

		static EventSerialiser()
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

		public string SerialisEvent<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			return JsonConvert.SerializeObject(@event, DefaultSettings);
		}

		public IEvent<TAuthenticationToken> DeserialisEvent(string @event)
		{
			return JsonConvert.DeserializeObject<IEvent<TAuthenticationToken>>(@event, DefaultSettings);
		}
	}
}