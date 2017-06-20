using System;
using System.Data.Linq.Mapping;
using System.Runtime.Serialization;
using Cqrs.Entities;

namespace Cqrs.Tests.Substitutes
{
	[Serializable]
	[DataContract]
	[Table(Name = "Orders")]
	public class OrderEntity : Entity
	{
		[DataMember]
		[Column]
		public override Guid Rsn { get; set; }

		[DataMember]
		[Column]
		public override int SortingOrder { get; set; }

		[DataMember]
		[Column]
		public override bool IsLogicallyDeleted { get; set; }

		[DataMember]
		[Column(Name = "OrderID", IsPrimaryKey = true, IsDbGenerated = true)]
		public virtual int OrderId { get; set; }

		[DataMember]
		[Column(Name = "CustomerID")]
		public virtual string CustomerId { get; set; }

		[DataMember]
		[Column(Name = "EmployeeID")]
		public virtual int? EmployeeId { get; set; }

		[DataMember]
		[Column]
		public virtual DateTime? OrderDate { get; set; }

		[DataMember]
		[Column]
		public virtual DateTime? RequiredDate { get; set; }

		[DataMember]
		[Column]
		public virtual DateTime? ShippedDate { get; set; }

		[DataMember]
		[Column(Name = "ShipVia")]
		public virtual int? ShipViaId { get; set; }

		[DataMember]
		[Column]
		public virtual decimal? Freight { get; set; }

		[DataMember]
		[Column]
		public virtual string ShipName { get; set; }

		[DataMember]
		[Column]
		public virtual string ShipAddress { get; set; }

		[DataMember]
		[Column]
		public virtual string ShipCity { get; set; }

		[DataMember]
		[Column]
		public virtual string ShipRegion { get; set; }

		[DataMember]
		[Column]
		public virtual string ShipPostalCode { get; set; }

		[DataMember]
		[Column]
		public virtual string ShipCountry { get; set; }
	}
}