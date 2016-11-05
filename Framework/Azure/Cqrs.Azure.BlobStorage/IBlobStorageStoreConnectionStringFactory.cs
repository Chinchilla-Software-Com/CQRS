#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;

namespace Cqrs.Azure.BlobStorage
{
	public interface IBlobStorageStoreConnectionStringFactory
	{
		IEnumerable<string> GetWritableConnectionStrings();

		string GetReadableConnectionString();

		string GetBaseContainerName();
	}
}