#if NET452
using Microsoft.ServiceBus.Messaging;
#endif
#if NETSTANDARD2_0
using Microsoft.Azure.ServiceBus;
using System.Runtime.Serialization;
using System.IO;
using System.Text;
using System.Xml;
#endif

namespace Cqrs.Azure.ServiceBus
{
	internal static class MessageExtensions
	{
#if NET452
		public static void AddUserProperty(this BrokeredMessage message, string key, object value)
		{
			message.Properties.Add(key, value);
		}
#endif
#if NETSTANDARD2_0
		public static void AddUserProperty(this Message message, string key, object value)
		{
			message.UserProperties.Add(key, value);
		}
#endif

#if NET452
		public static bool TryGetUserPropertyValue(this BrokeredMessage message, string key, out object value)
		{
			return message.Properties.TryGetValue(key, out value);
		}

		public static string GetBodyAsString(this BrokeredMessage message)
		{
			return message.GetBody<string>();
		}
#endif
#if NETSTANDARD2_0
		public static bool TryGetUserPropertyValue(this Message message, string key, out object value)
		{
			return message.UserProperties.TryGetValue(key, out value);
		}

		private static DataContractBinarySerializer Serialiser = new DataContractBinarySerializer(typeof(string));
		public static string GetBodyAsString(this Message message)
		{
			using (var stream = new MemoryStream(message.Body.Length))
			{
				stream.Write(message.Body, 0, message.Body.Length);
				stream.Flush();
				stream.Position = 0;
				return (string)Serialiser.ReadObject(stream);
			}
		}
#endif
	}
}