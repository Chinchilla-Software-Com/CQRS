#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;

namespace Cqrs.Azure.BlobStorage
{
	public interface IStorageStoreConnectionStringFactory
	{
		IEnumerable<string> GetWritableConnectionStrings();

		string GetReadableConnectionString();
	}
}