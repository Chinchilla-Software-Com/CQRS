using System;
using System.Collections.Generic;
using System.Linq;

namespace Cqrs.Repositories.Queries
{
	public static class QueryPredicateExtensions
	{
		public static TResult GetValue<TResult>(this KeyValuePair<string, object> keyValuePair)
		{
			return (TResult) keyValuePair.Value;
		}

		public static TResult GetValue<TResult>(this ISet<KeyValuePair<string, object>> collection, string key)
		{
			return collection.Single(kvp => kvp.Key == key).GetValue<TResult>();
		}

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

		public static TResult GetValue<TResult>(this QueryParameter queryParameter)
		{
			return (TResult)queryParameter.ParameterValue;
		}

		public static TResult GetValue<TResult>(this ISet<QueryParameter> collection, string parameterName)
		{
			return collection.Single(queryParameter => queryParameter.ParameterName == parameterName).GetValue<TResult>();
		}

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

		public static TResult GetValue<TResult>(this SortedSet<QueryParameter> collection, string parameterName)
		{
			return collection.Single(queryParameter => queryParameter.ParameterName == parameterName).GetValue<TResult>();
		}
	}
}