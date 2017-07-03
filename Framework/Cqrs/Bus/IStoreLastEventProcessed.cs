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
	[ServiceContract(Namespace = "https://getcqrs.net/Bus/StoreLastEventProcessed")]
	public interface IStoreLastEventProcessed
	{
		string EventLocation { get; set; }
	}
}