using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Cqrs.Entities
{
	[Serializable]
	[DataContract]
	public abstract class Entity : IEntity
	{
		[Required]
		[DataMember]
		public virtual Guid Rsn { get; set; }

		[DataMember]
		public virtual int SortingOrder { get; set; }

		[Required]
		[DataMember]
		public virtual bool IsLogicallyDeleted { get; set; }
	}
}