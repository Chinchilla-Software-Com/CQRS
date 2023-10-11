#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion


#if NETSTANDARD2_0
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Azure.Messaging.ServiceBus;
#else
using Microsoft.ServiceBus.Messaging;
#endif

namespace Cqrs.Azure.ServiceBus
{
	internal static class MessageExtensions
	{
#if NETSTANDARD2_0
		public static void AddUserProperty(this ServiceBusMessage message, string key, object value)
		{
			message.ApplicationProperties.Add(key, value);
		}
#else
		public static void AddUserProperty(this BrokeredMessage message, string key, object value)
		{
			message.Properties.Add(key, value);
		}
#endif

#if NETSTANDARD2_0
		public static bool TryGetUserPropertyValue(this ServiceBusReceivedMessage message, string key, out object value)
		{
			return message.ApplicationProperties.TryGetValue(key, out value);
		}
#else
		public static bool TryGetUserPropertyValue(this BrokeredMessage message, string key, out object value)
		{
			return message.Properties.TryGetValue(key, out value);
		}
#endif

#if NETSTANDARD2_0
		private static DataContractBinarySerializer Serialiser = new DataContractBinarySerializer(typeof(string));
		public static string GetBodyAsString(this ServiceBusReceivedMessage message)
		{
			byte[] rawStream = message.Body.ToArray();
			using (var stream = new MemoryStream(rawStream.Length))
			{
				stream.Write(rawStream, 0, rawStream.Length);
				stream.Flush();
				stream.Position = 0;
				try
				{
					return (string)Serialiser.ReadObject(stream);
				}
				catch (SerializationException)
				{
					stream.Position = 0;
					using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
						return reader.ReadToEnd();
				}
			}
		}
#else
		public static string GetBodyAsString(this BrokeredMessage message)
		{
			return message.GetBody<string>();
		}
#endif
	}
}