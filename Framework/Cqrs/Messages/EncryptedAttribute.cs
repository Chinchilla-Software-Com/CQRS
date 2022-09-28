#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.DataStores;
using Cqrs.Events;
using System;

namespace Cqrs.Messages
{
	/// <summary>
	/// Provides a mechanism to configure encryption for sensitive data.
	/// This is useful if you have events that contain sensitive data that you don't want in plain text within a <see cref="IDataStore{TData}"/>, <see cref="IEventStore{TAuthenticationToken}"/> or even in a queue as part of a distributed system as part of transit.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
	public class EncryptedAttribute : Attribute
	{
		/// <summary>
		/// The name of the encryption key to use, obtained via configuration
		/// </summary>
		public string KeyName { get; set; }
	}
}