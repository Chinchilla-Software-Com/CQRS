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
#if NET40_OR_GREATER
using System.Data.Linq.Mapping;
#endif
using Cqrs.DataStores;

namespace Cqrs.Entities
{
	/// <summary>
	/// A projection/entity.
	/// </summary>
	[Serializable]
	[DataContract]
	public abstract class Entity : IEntity
	{
		/// <summary>
		/// The identifier of the <see cref="IEntity"/>.
		/// </summary>
		[Required]
		[DataMember]
#if NET40_OR_GREATER
		[Column(IsPrimaryKey = true)]
#endif
		public virtual Guid Rsn { get; set; }

		/// <summary>
		/// The order of this <see cref="IEntity"/> to sort by, by default.
		/// </summary>
		[DataMember]
#if NET40_OR_GREATER
		[Column]
#endif
		public virtual int SortingOrder { get; set; }

		/// <summary>
		/// Indicates if this <see cref="IEntity"/> has been deleted, but not removed from the <see cref="IDataStore{TData}"/>,
		/// this way entities can be retrieved so an un-deleted operation can be triggered.
		/// </summary>
		[Required]
		[DataMember]
#if NET40_OR_GREATER
		[Column]
#endif
		public virtual bool IsDeleted { get; set; }
	}
}