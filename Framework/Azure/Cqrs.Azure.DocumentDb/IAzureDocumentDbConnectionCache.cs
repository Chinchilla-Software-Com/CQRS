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
	public interface IAzureDocumentDbConnectionCache
	{
		bool TryGetClient(string key, out DocumentClient client);

		void SetClient(string key, DocumentClient client);

		bool TryGetDatabase(string key, out Database database);

		void SetDatabase(string key, Database database);

		bool TryGetDocumentCollection(string key, out DocumentCollection documentCollection);

		void SetDocumentCollection(string key, DocumentCollection documentCollection);
	}
}