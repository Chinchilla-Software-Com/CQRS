#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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