using System;

namespace Cqrs.Repositories.Queries
{
	public class QueryParameter : IComparable<QueryParameter>
	{
		public QueryParameter() { }

		public QueryParameter(string parameterName, object parameterValue)
		{
			ParameterName = parameterName;
			ParameterValue = parameterValue;
		}

		public string ParameterName { get; set; }

		public object ParameterValue { get; set; }

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
			return System.String.Compare(ParameterName, other.ParameterName, StringComparison.Ordinal);
		}

		#endregion
	}
}