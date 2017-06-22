namespace KendoUI.Northwind.Dashboard.Code.Events
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Cqrs.Events;

	public class OrderCreated : IEvent<string>
	{
		#region Implementation of IEvent

		[DataMember]
		public Guid Id
		{
			get
			{
				return Rsn;
			}
			set
			{
				Rsn = value;
			}
		}

		[DataMember]
		public int Version { get; set; }

		[DataMember]
		public DateTimeOffset TimeStamp { get; set; }

		#endregion

		#region Implementation of IMessageWithAuthenticationToken<string>

		public string AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IMessage

		public Guid CorrelationId { get; set; }

		/// <summary>
		/// The originating framework this message was sent from.
		/// </summary>
		public string OriginatingFramework { get; set; }

		/// <summary>
		/// The frameworks this <see cref="T:Cqrs.Messages.IMessage"/> has been delivered to/sent via already.
		/// </summary>
		public IEnumerable<string> Frameworks { get; set; }

		#endregion

		[DataMember]
		public Guid Rsn { get; set; }

		[DataMember]
		public int OrderId { get; private set; }

		[DataMember]
		public string CustomerId { get; private set; }

		[DataMember]
		public int? EmployeeId { get; private set; }

		[DataMember]
		public DateTime? OrderDate { get; private set; }

		[DataMember]
		public DateTime? RequiredDate { get; private set; }

		[DataMember]
		public DateTime? ShippedDate { get; private set; }

		[DataMember]
		public int? ShipViaId { get; private set; }

		[DataMember]
		public decimal? Freight { get; private set; }

		[DataMember]
		public string ShipName { get; private set; }

		[DataMember]
		public string ShipAddress { get; private set; }

		[DataMember]
		public string ShipCity { get; private set; }

		[DataMember]
		public string ShipRegion { get; private set; }

		[DataMember]
		public string ShipPostalCode { get; private set; }

		[DataMember]
		public string ShipCountry { get; private set; }


		public OrderCreated(Guid rsn, int orderId, string customerId, int? employeeId, DateTime? orderDate, DateTime? requiredDate, DateTime? shippedDate, int? shipViaId, decimal? freight, string shipName, string shipAddress, string shipCity, string shipRegion, string shipPostalCode, string shipCountry)
		{
			Rsn = rsn;
			OrderId = orderId;
			CustomerId = customerId;
			EmployeeId = employeeId;
			OrderDate = orderDate;
			RequiredDate = requiredDate;
			ShippedDate = shippedDate;
			ShipViaId = shipViaId;
			Freight = freight;
			ShipName = shipName;
			ShipAddress = shipAddress;
			ShipCity = shipCity;
			ShipRegion = shipRegion;
			ShipPostalCode = shipPostalCode;
			ShipCountry = shipCountry;
		}
	}
}