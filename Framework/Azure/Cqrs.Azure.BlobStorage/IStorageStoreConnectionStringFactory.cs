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
	public interface IStorageStoreConnectionStringFactory
	{
		IEnumerable<string> GetWritableConnectionStrings();

		string GetReadableConnectionString();
	}
}