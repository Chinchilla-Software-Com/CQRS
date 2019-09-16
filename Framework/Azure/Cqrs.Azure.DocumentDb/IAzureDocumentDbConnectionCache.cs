#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb
{
	/// <summary>
	/// A cache manager for DocumentDB clients, databases and collections.
	/// </summary>
	public interface IAzureDocumentDbConnectionCache
	{
		/// <summary>
		/// Gets the <see cref="DocumentClient"/>.
		/// </summary>
		/// <param name="key">The name of the <see cref="DocumentClient"/> to get.</param>
		/// <param name="client">If the <see cref="DocumentClient"/> is found, it is returned here; otherwise null is returned. This parameter is passed uninitialized.</param>
		/// <returns>true if the  <see cref="DocumentClient"/> is found; otherwise, false.</returns>
		bool TryGetClient(string key, out DocumentClient client);

		/// <summary>
		/// Sets the provided <paramref name="client"/>.
		/// </summary>
		/// <param name="key">The name of the <see cref="DocumentClient"/> to get.</param>
		/// <param name="client">The <see cref="DocumentClient"/> to set.</param>
		void SetClient(string key, DocumentClient client);

		/// <summary>
		/// Gets the <see cref="Database"/>.
		/// </summary>
		/// <param name="key">The name of the <see cref="Database"/> to get.</param>
		/// <param name="database">If the <see cref="Database"/> is found, it is returned here; otherwise null is returned. This parameter is passed uninitialized.</param>
		/// <returns>true if the  <see cref="Database"/> is found; otherwise, false.</returns>
		bool TryGetDatabase(string key, out Database database);

		/// <summary>
		/// Sets the provided <paramref name="database"/>.
		/// </summary>
		/// <param name="key">The name of the <see cref="Database"/> to get.</param>
		/// <param name="database">The <see cref="Database"/> to set.</param>
		void SetDatabase(string key, Database database);

		/// <summary>
		/// Gets the <see cref="DocumentCollection"/>.
		/// </summary>
		/// <param name="key">The name of the <see cref="DocumentCollection"/> to get.</param>
		/// <param name="documentCollection">If the <see cref="DocumentCollection"/> is found, it is returned here; otherwise null is returned. This parameter is passed uninitialized.</param>
		/// <returns>true if the  <see cref="DocumentCollection"/> is found; otherwise, false.</returns>
		bool TryGetDocumentCollection(string key, out DocumentCollection documentCollection);

		/// <summary>
		/// Sets the provided <paramref name="documentCollection"/>.
		/// </summary>
		/// <param name="key">The name of the <see cref="DocumentCollection"/> to get.</param>
		/// <param name="documentCollection">The <see cref="DocumentCollection"/> to set.</param>
		void SetDocumentCollection(string key, DocumentCollection documentCollection);
	}
}