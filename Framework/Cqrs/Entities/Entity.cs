using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Data.Linq.Mapping;

namespace Cqrs.Entities
{
	[Serializable]
	[DataContract]
	public abstract class Entity : IEntity
	{
		[Required]
		[DataMember]
		[Column(IsPrimaryKey = true)]
		public virtual Guid Rsn { get; set; }

		[DataMember]
		[Column]
		public virtual int SortingOrder { get; set; }

		[Required]
		[DataMember]
		[Column]
		public virtual bool IsLogicallyDeleted { get; set; }
	}
}