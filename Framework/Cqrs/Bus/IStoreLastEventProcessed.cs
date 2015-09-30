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
	[ServiceContract(Namespace = "http://cqrs.co.nz/Bus/StoreLastEventProcessed")]
	public interface IStoreLastEventProcessed
	{
		string EventLocation { get; set; }
	}
}