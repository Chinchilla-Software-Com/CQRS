#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using EventStore.ClientAPI;

namespace Cqrs.EventStore
{
	/// <summary>
	/// Creates instances of <see cref="IEventStoreConnection"/>.
	/// </summary>
	public interface IEventStoreConnectionHelper
	{
		/// <summary>
		/// Gets a <see cref="IEventStoreConnection"/>
		/// </summary>
		IEventStoreConnection GetEventStoreConnection();
	}
}