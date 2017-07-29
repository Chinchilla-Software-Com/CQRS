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
	/// Sorting information.
	/// </summary>
	/// <typeparam name="TSortBy">The <see cref="Type"/> to sort by</typeparam>.
	public class SortParameter<TSortBy>
	{
		/// <summary>
		/// Sort direction.
		/// </summary>
		public enum SortParameterDirectionType
		{
			/// <summary>
			/// Sort Ascending.
			/// </summary>
			Ascending = 0,

			/// <summary>
			/// Sort Descending.
			/// </summary>
			Descending = 2
		}

		/// <summary>
		/// What to sort by.
		/// </summary>
		public TSortBy SortBy { get; set; }

		/// <summary>
		/// The direction to sort.
		/// </summary>
		public SortParameterDirectionType Direction { get; set; }
	}
}