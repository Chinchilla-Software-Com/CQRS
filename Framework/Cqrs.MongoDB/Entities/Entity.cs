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
using Cqrs.Entities;
using MongoDB.Bson.Serialization.Attributes;

namespace Cqrs.MongoDB.Entities
{
	/// <summary>
	/// A projection/entity especially designed to work with MongoDB.
	/// </summary>
	[Serializable]
	[DataContract]
	public abstract class MongoEntity : Entity
	{
		/// <summary>
		/// The identifier of the <see cref="IEntity"/>.
		/// </summary>
		[Required]
		[BsonId]
		[DataMember]
		public override Guid Rsn { get; set; }

		/// <summary>
		/// The order of this <see cref="IEntity"/> to sort by, by default.
		/// </summary>
		[DataMember]
		public override int SortingOrder { get; set; }

		/// <summary>
		/// Indicates if this <see cref="IEntity"/> has been deleted, but not removed from the <see cref="IDataStore{TData}"/>,
		/// this way entities can be retrieved so an un-deleted operation can be triggered.
		/// </summary>
		[Required]
		[DataMember]
		public override bool IsDeleted { get; set; }
	}
}