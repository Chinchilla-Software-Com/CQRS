#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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
	}
}