#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cqrs.Azure.DocumentDb
{
	/// <summary>
	/// A helper for Azure Document DB.
	/// </summary>
	public interface IAzureDocumentDbHelper
	{
		/// <summary>
		/// Gets a <see cref="Database"/> creating one if it doesn't already exist.
		/// </summary>
		/// <param name="client">The <see cref="DocumentClient"/> to use.</param>
		/// <param name="databaseName">The name of the database to get.</param>
		Task<Database> CreateOrReadDatabase(DocumentClient client, string databaseName);

		/// <summary>
		/// Gets a <see cref="DocumentCollection"/> creating one if it doesn't already exist.
		/// </summary>
		/// <param name="client">The <see cref="DocumentClient"/> to use.</param>
		/// <param name="database">The <see cref="Database"/> to look in.</param>
		/// <param name="collectionName">The name of the collection to get.</param>
		/// <param name="uniqiueIndexPropertyNames">Any unique properties the collection should enforce.</param>
		Task<DocumentCollection> CreateOrReadCollection(DocumentClient client, Database database, string collectionName, string[] uniqiueIndexPropertyNames = null);

		/// <summary>
		/// Execute the provided <paramref name="func"/> in a fault tolerant way.
		/// </summary>
		/// <param name="func">The <see cref="Func{T}"/> to execute.</param>
		T ExecuteFaultTollerantFunction<T>(Func<T> func);

		/// <summary>
		/// Execute the provided <paramref name="func"/> in a fault tolerant way.
		/// </summary>
		/// <param name="func">The <see cref="Action"/> to execute.</param>
		void ExecuteFaultTollerantFunction(Action func);
	}
}