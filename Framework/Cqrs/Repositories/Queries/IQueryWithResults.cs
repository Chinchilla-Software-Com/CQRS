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
	/// A query that will produce a result
	/// </summary>
	/// <typeparam name="TResult">The <see cref="Type"/> of data in the result collection.</typeparam>
	public interface IQueryWithResults<out TResult>
	{
		/// <summary>
		/// The resulting of executing the <see cref="QueryStrategy"/>.
		/// </summary>
		TResult Result { get; }
	}
}