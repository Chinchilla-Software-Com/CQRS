#if NET452
using Microsoft.ServiceBus.Messaging;
#endif
#if NETSTANDARD2_0
using Microsoft.Azure.ServiceBus;
using System.Runtime.Serialization;
using System.IO;
using System.Text;
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

		private static DataContractSerializer Serialiser = new DataContractSerializer(typeof(string));
		public static string GetBodyAsString(this Message message)
		{
			try
			{
				return Encoding.UTF8.GetString(message.Body, 0, message.Body.Length);
			}
			catch
			{
				using (var stream = new MemoryStream(message.Body))
					return (string)Serialiser.ReadObject(stream);
			}
		}
#endif
	}
}