#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Azure.BlobStorage
{
	public interface IBlobStorageStoreConnectionStringFactory : IStorageStoreConnectionStringFactory
	{
		string GetBaseContainerName();
	}
}