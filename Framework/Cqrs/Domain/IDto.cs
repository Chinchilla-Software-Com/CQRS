#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;

namespace Cqrs.Domain
{
	/// <summary>
	/// A basic data transfer object suitable for CRUD operations.
	/// </summary>
	public interface IDto
	{
		/// <summary>
		/// The identifier of this <see cref="IDto"/>.
		/// </summary>
		[DataMember]
		Guid Id { get; set; }
	}
}