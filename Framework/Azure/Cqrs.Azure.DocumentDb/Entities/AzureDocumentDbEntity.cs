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

namespace Cqrs.Azure.DocumentDb.Entities
{
	/// <summary>
	/// A projection/entity especially designed to work with Azure DocumentDB (CosmosDB).
	/// </summary>
	[Serializable]
	[DataContract]
	public abstract class AzureDocumentDbEntity : Entity
	{
		/// <summary>
		/// The identifier of the <see cref="IEntity"/>.
		/// </summary>
		[Required]
		[DataMember]
		public override Guid Rsn { get; set; }

		/// <summary>
		/// The internal identifier of the <see cref="IEntity"/> used within Azure DocumentDB (CosmosDB).
		/// </summary>
		[Required]
		[DataMember]
		public virtual string id
		{
			get { return string.Format("{0}/{1:N}", GetType().FullName, Rsn); }
			set
			{
				Rsn = new Guid(value.Split('/')[1]);
			}
		}

		/// <summary>
		/// The name of the <see cref="Type"/> of this <see cref="IEntity"/>.
		/// </summary>
		[Required]
		[DataMember]
		public virtual string type
		{
			get { return GetType().FullName; }
			set{ }
		}

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
		public override bool IsLogicallyDeleted { get; set; }
	}
}