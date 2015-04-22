using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Cqrs.Entities
{
	public interface IEntity
	{
		[Required]
		[DataMember]
		Guid Rsn { get; set; }

		[DataMember]
		int SortingOrder { get; set; }

		[Required]
		[DataMember]
		bool IsLogicallyDeleted { get; set; }
	}
}