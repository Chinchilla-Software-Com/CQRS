#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Cqrs.DataStores;

namespace Cqrs.Entities
{
	/// <summary>
	/// A projection/entity.
	/// </summary>
	public interface IEntity
	{
		/// <summary>
		/// The identifier of the <see cref="IEntity"/>.
		/// </summary>
		[Required]
		[DataMember]
		Guid Rsn { get; set; }

		/// <summary>
		/// The order of this <see cref="IEntity"/> to sort by, by default.
		/// </summary>
		[DataMember]
		int SortingOrder { get; set; }

		/// <summary>
		/// Indicates if this <see cref="IEntity"/> has been deleted, but not removed from the <see cref="IDataStore{TData}"/>,
		/// this way entities can be retrieved so an un-deleted operation can be triggered.
		/// </summary>
		[Required]
		[DataMember]
		bool IsLogicallyDeleted { get; set; }
	}
}