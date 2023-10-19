#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

#if NETSTANDARD2_0 || NET6_0
using Microsoft.Azure.EventHubs;
#else
using Microsoft.ServiceBus.Messaging;
#endif

namespace Cqrs.Azure.ServiceBus
{
	internal static class EventHubMessageExtensions
	{
		public static void AddUserProperty(this EventData message, string key, object value)
		{
			message.Properties.Add(key, value);
		}

		public static bool TryGetUserPropertyValue(this EventData message, string key, out object value)
		{
			return message.Properties.TryGetValue(key, out value);
		}
	}
}