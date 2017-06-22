namespace KendoUI.Northwind.Dashboard.Code.Entities
{
	using System;
	using System.Data.Linq.Mapping;
	using System.Runtime.Serialization;
	using Cqrs.Entities;

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

namespace KendoUI.Northwind.Dashboard.Models
{
	using System;
	using System.ComponentModel.DataAnnotations;
	using Code.Entities;
	using Cqrs.Entities;

	public partial class OrderViewModel
	{
		[ScaffoldColumn(false)]
		public Guid Rsn { get; set; }

		/// <summary>
		/// Explicit cast of <see cref="OrderEntity"/> to <see cref="OrderViewModel"/>
		/// </summary>
		/// <param name="entity">A <see cref="OrderEntity"/> <see cref="Entity"/> to convert</param>
		/// <returns>A <see cref="OrderViewModel"/> object</returns>
		public static explicit operator OrderViewModel(OrderEntity entity)
		{
			OrderViewModel result = new OrderViewModel
			{
				Rsn = entity.Rsn,
				CustomerID = entity.CustomerId,
				OrderID = entity.OrderId,
				EmployeeID = entity.EmployeeId,
				OrderDate = entity.OrderDate,
				ShipCountry = entity.ShipCountry,
				ShipVia = entity.ShipViaId,
				ShippedDate = entity.ShippedDate,
				ShipName = entity.ShipName,
				ShipAddress = entity.ShipAddress,
				ShipCity = entity.ShipCity,
				ShipPostalCode = entity.ShipPostalCode
			};

			return result;
		}
	}
}