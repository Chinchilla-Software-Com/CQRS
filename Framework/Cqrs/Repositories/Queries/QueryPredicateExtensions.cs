#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cqrs.Repositories.Queries
{
	/// <summary>
	/// A collection of extension methods for working with queries.
	/// </summary>
	public static class QueryPredicateExtensions
	{
		/// <summary>
		/// Gets the value from the provided <paramref name="keyValuePair"/> cast to <typeparamref name="TResult"/>.
		/// </summary>
		/// <remarks>
		/// Gets the <see cref="KeyValuePair{TKey,TValue}.Value"/> from the provided <paramref name="keyValuePair"/> cast to <typeparamref name="TResult"/>.
		/// </remarks>
		public static TResult GetValue<TResult>(this KeyValuePair<string, object> keyValuePair)
		{
			return (TResult) keyValuePair.Value;
		}

		/// <summary>
		/// Gets the value from the provided <paramref name="collection"/> cast to <typeparamref name="TResult"/>.
		/// </summary>
		/// <remarks>
		/// Filters the provided <paramref name="collection"/> where the <see cref="KeyValuePair{TKey,TValue}.Key"/> equals the provided <paramref name="key"/>
		/// then gets the <see cref="KeyValuePair{TKey,TValue}.Value"/> cast to <typeparamref name="TResult"/>.
		/// </remarks>
		public static TResult GetValue<TResult>(this ISet<KeyValuePair<string, object>> collection, string key)
		{
			return collection.Single(kvp => kvp.Key == key).GetValue<TResult>();
		}

		/// <summary>
		/// Gets the value from the provided <paramref name="collection"/> cast to <typeparamref name="TResult"/>.
		/// </summary>
		/// <remarks>
		/// Gets the <see cref="KeyValuePair{TKey,TValue}"/> at index <paramref name="index"/> from the provided <paramref name="collection"/>
		/// then gets the <see cref="KeyValuePair{TKey,TValue}.Value"/> cast to <typeparamref name="TResult"/>.
		/// </remarks>
		public static TResult GetValue<TResult>(this SortedSet<KeyValuePair<string, object>> collection, int index)
		{
			int collectionIndex = 0;
			foreach (KeyValuePair<string, object> keyValuePair in collection)
			{
				if (index < collectionIndex++)
					continue;
				return keyValuePair.GetValue<TResult>();
			}
			throw new IndexOutOfRangeException();
		}

		/// <summary>
		/// Gets the value from the provided <paramref name="queryParameter"/> cast to <typeparamref name="TResult"/>.
		/// </summary>
		/// <remarks>
		/// Gets the <see cref="QueryParameter.ParameterValue"/> cast to <typeparamref name="TResult"/>.
		/// </remarks>
		public static TResult GetValue<TResult>(this QueryParameter queryParameter)
		{
			return (TResult)queryParameter.ParameterValue;
		}

		/// <summary>
		/// Gets the value from the provided <paramref name="collection"/> cast to <typeparamref name="TResult"/>.
		/// </summary>
		/// <remarks>
		/// Filters the provided <paramref name="collection"/> where the <see cref="QueryParameter.ParameterName"/> equals the provided <paramref name="parameterName"/>
		/// then gets the <see cref="QueryParameter.ParameterValue"/> cast to <typeparamref name="TResult"/>.
		/// </remarks>
		public static TResult GetValue<TResult>(this ISet<QueryParameter> collection, string parameterName)
		{
			return collection.Single(queryParameter => queryParameter.ParameterName == parameterName).GetValue<TResult>();
		}

		/// <summary>
		/// Gets the value from the provided <paramref name="collection"/> cast to <typeparamref name="TResult"/>.
		/// </summary>
		/// <remarks>
		/// Gets the <see cref="QueryParameter"/> at index <paramref name="index"/> from the provided <paramref name="collection"/>
		/// then gets the <see cref="QueryParameter.ParameterValue"/> cast to <typeparamref name="TResult"/>.
		/// </remarks>
		public static TResult GetValue<TResult>(this SortedSet<QueryParameter> collection, int index)
		{
			int collectionIndex = 0;
			foreach (QueryParameter queryParameter in collection)
			{
				if (index < collectionIndex++)
					continue;
				return queryParameter.GetValue<TResult>();
			}
			throw new IndexOutOfRangeException();
		}

		/// <summary>
		/// Gets the value from the provided <paramref name="collection"/> cast to <typeparamref name="TResult"/>.
		/// </summary>
		/// <remarks>
		/// Filters the provided <paramref name="collection"/> where the <see cref="QueryParameter.ParameterName"/> equals the provided <paramref name="parameterName"/>
		/// then gets the <see cref="QueryParameter.ParameterValue"/> cast to <typeparamref name="TResult"/>.
		/// </remarks>
		public static TResult GetValue<TResult>(this SortedSet<QueryParameter> collection, string parameterName)
		{
			return collection.Single(queryParameter => queryParameter.ParameterName == parameterName).GetValue<TResult>();
		}
	}
}