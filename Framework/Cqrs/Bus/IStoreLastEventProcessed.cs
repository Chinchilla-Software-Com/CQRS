#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.ServiceModel;

namespace Cqrs.Bus
{
	/// <summary>
	/// Indicates the position in a store where the stream has been read up to.
	/// </summary>
	[ServiceContract(Namespace = "https://getcqrs.net/Bus/StoreLastEventProcessed")]
	public interface IStoreLastEventProcessed
	{
		/// <summary>
		/// The location within the store where the stream has been read up to.
		/// </summary>
		string EventLocation { get; set; }
	}
}