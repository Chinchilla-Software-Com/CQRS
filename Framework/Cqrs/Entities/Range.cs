#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Entities
{
	/// <summary>
	/// A range object for collecting a lower and upper limit, such as a number or date range.
	/// </summary>
	public class Range<TPrimitive>
		where TPrimitive : struct
	{
		/// <summary>
		/// The lower limit such as a from <see cref="DateTime"/>.
		/// </summary>
		public TPrimitive? From { get; set; }

		/// <summary>
		/// The upper limit such as a to <see cref="DateTime"/>.
		/// </summary>
		public TPrimitive? To { get; set; }

		/// <summary>
		/// Is the value of <see cref="From"/> inclusive or not. Defaults to true.
		/// </summary>
		public bool IsFromInclusive { get; set; }

		/// <summary>
		/// Is the value of <see cref="To"/> inclusive or not. Defaults to true.
		/// </summary>
		public bool IsToInclusive { get; set; }
	}
}