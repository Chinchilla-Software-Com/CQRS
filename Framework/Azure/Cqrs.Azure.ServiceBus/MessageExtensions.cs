#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion


#if NETSTANDARD2_0 || NET5_0_OR_GREATER
using Microsoft.Azure.ServiceBus;
using System.IO;
#else
using Microsoft.ServiceBus.Messaging;
#endif

namespace Cqrs.Azure.ServiceBus
{
	internal static class MessageExtensions
	{
#if NETSTANDARD2_0 || NET5_0_OR_GREATER
		public static void AddUserProperty(this Message message, string key, object value)
		{
			message.UserProperties.Add(key, value);
		}
#else
		public static void AddUserProperty(this BrokeredMessage message, string key, object value)
		{
			message.Properties.Add(key, value);
		}
#endif

#if NETSTANDARD2_0 || NET5_0_OR_GREATER
		public static bool TryGetUserPropertyValue(this Message message, string key, out object value)
		{
			return message.UserProperties.TryGetValue(key, out value);
		}
#else
		public static bool TryGetUserPropertyValue(this BrokeredMessage message, string key, out object value)
		{
			return message.Properties.TryGetValue(key, out value);
		}
#endif

#if NETSTANDARD2_0 || NET5_0_OR_GREATER
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
#else
		public static string GetBodyAsString(this BrokeredMessage message)
		{
			return message.GetBody<string>();
		}
#endif
	}
}