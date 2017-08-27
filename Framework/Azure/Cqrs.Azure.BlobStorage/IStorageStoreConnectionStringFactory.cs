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
	/// <summary>
	/// A factory for getting connection strings and container names.
	/// </summary>
	public interface IStorageStoreConnectionStringFactory
	{
		/// <summary>
		/// Gets all writeable connection strings.
		/// </summary>
		IEnumerable<string> GetWritableConnectionStrings();

		/// <summary>
		/// Gets the readable connection string.
		/// </summary>
		string GetReadableConnectionString();
	}
}