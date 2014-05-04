using System;
using System.Text;
using Newtonsoft.Json;

namespace Cqrs.EventStore.Bus
{
	public class EventConverter
	{
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