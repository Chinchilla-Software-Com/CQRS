using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Cqrs.Azure.DocumentDb.Entities
{
	[Serializable]
	[DataContract]
	public abstract class AzureDocumentDbEntity : Cqrs.Entities.Entity
	{
		[Required]
		[DataMember]
		public override Guid Rsn { get; set; }

		[Required]
		[DataMember]
		public virtual Guid id
		{
			get { return Rsn; }
			set { Rsn = value; }
		}

		[DataMember]
		public override int SortingOrder { get; set; }

		[Required]
		[DataMember]
		public override bool IsLogicallyDeleted { get; set; }
	}
}