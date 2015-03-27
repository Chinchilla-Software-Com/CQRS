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
		public virtual string id
		{
			get { return string.Format("{0}/{1:N}", GetType().FullName, Rsn); }
			set
			{
				Rsn = new Guid(value.Split('/')[1]);
			}
		}

		[Required]
		[DataMember]
		public virtual string type
		{
			get { return GetType().FullName; }
			set{ }
		}

		[DataMember]
		public override int SortingOrder { get; set; }

		[Required]
		[DataMember]
		public override bool IsLogicallyDeleted { get; set; }
	}
}