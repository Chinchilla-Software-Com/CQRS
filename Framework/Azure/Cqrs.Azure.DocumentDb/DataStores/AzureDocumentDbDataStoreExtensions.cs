#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using Cqrs.Azure.DocumentDb.DataStores;
using Cqrs.Azure.DocumentDb.Entities;

namespace System.Linq
{
	/// <summary>
	/// Provides a set of static (Shared in Visual Basic) methods for querying objects that inherit <see cref="AzureDocumentDbDataStore{TData}"/>.
	/// </summary>
	public static class AzureDocumentDbDataStoreExtensions
	{
		/// <summary>
		/// Returns the only element of the sequence, and throws an exception if there is not exactly one element in the sequence.
		/// </summary>
		/// <param name="source">The sequence to type as <see cref="IEnumerable{T}"/>.</param>
		/// <typeparam name="TData">The type of the elements of source.</typeparam>
		/// <returns>The single element of the sequence.</returns>
		/// <exception cref="InvalidOperationException">The sequence contains more than one element, or the sequence is empty.</exception>
		public static TData Single<TData>(this AzureDocumentDbDataStore<TData> source)
			where TData : AzureDocumentDbEntity
		{
			return source.AsEnumerable().ToList().Single();
		}

		/// <summary>
		/// Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.
		/// </summary>
		/// <param name="predicate">A function to test an element for a condition.</param>
		/// <param name="source">The sequence to type as <see cref="IEnumerable{T}"/>.</param>
		/// <typeparam name="TData">The type of the elements of source.</typeparam>
		/// <returns>The single element of the sequence.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
		/// <exception cref="InvalidOperationException">The sequence is empty.</exception>
		public static TData Single<TData>(this AzureDocumentDbDataStore<TData> source, Func<TData, bool> predicate)
			where TData : AzureDocumentDbEntity
		{
			IList<TData> result = source.Where(predicate).AsEnumerable().ToList();
			return result.Single();
		}

		/// <summary>
		/// Returns the only element of the sequence, or a default value if the sequence is empty; this method throws an exception if there is more than one element in the sequence.
		/// </summary>
		/// <param name="source">The sequence to type as <see cref="IEnumerable{T}"/>.</param>
		/// <typeparam name="TData">The type of the elements of source.</typeparam>
		/// <returns>The single element of the sequence.</returns>
		public static TData SingleOrDefault<TData>(this AzureDocumentDbDataStore<TData> source)
			where TData : AzureDocumentDbEntity
		{
			return source.AsEnumerable().ToList().SingleOrDefault();
		}

		/// <summary>
		/// Returns the only element of the sequence that satisfies a specified condition or a default value if no such element exists; this method throws an exception if more than one element satisfies the condition.
		/// </summary>
		/// <param name="predicate">A function to test an element for a condition.</param>
		/// <param name="source">The sequence to type as <see cref="IEnumerable{T}"/>.</param>
		/// <typeparam name="TData">The type of the elements of source.</typeparam>
		/// <returns>The single element of the sequence.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
		public static TData SingleOrDefault<TData>(this AzureDocumentDbDataStore<TData> source, Func<TData, bool> predicate)
			where TData : AzureDocumentDbEntity
		{
			IList<TData> result = source.Where(predicate).AsEnumerable().ToList();
			return result.SingleOrDefault();
		}

		/// <summary>
		/// Returns the first element of the sequence.
		/// </summary>
		/// <param name="source">The sequence to type as <see cref="IEnumerable{T}"/>.</param>
		/// <typeparam name="TData">The type of the elements of source.</typeparam>
		/// <returns>The first element of the sequence.</returns>
		/// <exception cref="InvalidOperationException">The sequence contains more than one element, or the sequence is empty.</exception>
		public static TData First<TData>(this AzureDocumentDbDataStore<TData> source)
			where TData : AzureDocumentDbEntity
		{
			return source.AsEnumerable().ToList().First();
		}

		/// <summary>
		/// Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.
		/// </summary>
		/// <param name="predicate">A function to test an element for a condition.</param>
		/// <param name="source">The sequence to type as <see cref="IEnumerable{T}"/>.</param>
		/// <typeparam name="TData">The type of the elements of source.</typeparam>
		/// <returns>The first element of the sequence.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
		/// <exception cref="InvalidOperationException">The sequence is empty.</exception>
		public static TData First<TData>(this AzureDocumentDbDataStore<TData> source, Func<TData, bool> predicate)
			where TData : AzureDocumentDbEntity
		{
			IList<TData> result = source.Where(predicate).AsEnumerable().ToList();
			return result.First();
		}

		/// <summary>
		/// Returns the first element of the sequence that satisfies a condition or a default value if no such element is found.
		/// </summary>
		/// <param name="source">The sequence to type as <see cref="IEnumerable{T}"/>.</param>
		/// <typeparam name="TData">The type of the elements of source.</typeparam>
		/// <returns>The first element of the sequence.</returns>
		public static TData FirstOrDefault<TData>(this AzureDocumentDbDataStore<TData> source)
			where TData : AzureDocumentDbEntity
		{
			return source.AsEnumerable().ToList().FirstOrDefault();
		}

		/// <summary>
		/// Returns the first element of a sequence, or a default value if the sequence contains no elements.
		/// </summary>
		/// <param name="predicate">A function to test an element for a condition.</param>
		/// <param name="source">The sequence to type as <see cref="IEnumerable{T}"/>.</param>
		/// <typeparam name="TData">The type of the elements of source.</typeparam>
		/// <returns>The first element of the sequence.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
		public static TData FirstOrDefault<TData>(this AzureDocumentDbDataStore<TData> source, Func<TData, bool> predicate)
			where TData : AzureDocumentDbEntity
		{
			IList<TData> result = source.Where(predicate).AsEnumerable().ToList();
			return result.FirstOrDefault();
		}
	}
}