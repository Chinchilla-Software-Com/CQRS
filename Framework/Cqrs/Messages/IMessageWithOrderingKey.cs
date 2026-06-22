#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Messages
{
	/// <summary>
	/// A message such as an event or command with a key to aid in ordering where supported.
	/// </summary>
	public interface IMessageWithOrderingKey
	{
		/// <summary>
		/// The key to use to aid in ordering.
		/// Usually this would be a combination of entity type and entity id. When combined this creates a partition for ordering of messages to occur.
		/// </summary>
		string OrderKey { get; set; }
	}
}