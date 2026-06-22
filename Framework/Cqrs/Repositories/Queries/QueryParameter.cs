#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Repositories.Queries
{
	/// <summary>
	/// Information about a parameter pass to a function in a <see cref="IQueryStrategy"/>.
	/// </summary>
	public class QueryParameter : IComparable<QueryParameter>
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="QueryParameter"/>
		/// </summary>
		public QueryParameter() { }

		/// <summary>
		/// Instantiates a new instance of <see cref="QueryParameter"/>
		/// </summary>
		public QueryParameter(string parameterName, object parameterValue)
		{
			ParameterName = parameterName;
			ParameterValue = parameterValue;
		}

		/// <summary>
		/// The name of the parameter.
		/// </summary>
		public string ParameterName { get; set; }

		/// <summary>
		/// The value of the parameter.
		/// </summary>
		public object ParameterValue { get; set; }

		/// <summary>
		/// Returns <see cref="ParameterValue"/> cast to <typeparamref name="T"/>.
		/// </summary>
		public T GetParameterValue<T>()
		{
			return (T) ParameterValue;
		}

		#region Implementation of IComparable

		/// <summary>
		/// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than <paramref name="obj"/>. Zero This instance is equal to <paramref name="obj"/>. Greater than zero This instance is greater than <paramref name="obj"/>. 
		/// </returns>
		/// <param name="obj">An object to compare with this instance. </param>
		/// <exception cref="T:System.ArgumentException"><paramref name="obj"/> is not the same type as this instance. </exception>
		public int CompareTo(object obj)
		{
			var other = obj as QueryParameter;
			if (other != null)
				return CompareTo(other);
			return -1;
		}

		#endregion

		#region Implementation of IComparable<in QueryParameter>

		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public int CompareTo(QueryParameter other)
		{
			return string.Compare(ParameterName, other.ParameterName, StringComparison.Ordinal);
		}

		#endregion
	}
}