namespace KendoUI.Northwind.Dashboard.Code.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Cqrs.Commands;
	using Cqrs.Entities;
	using Models;

	/// <summary>
	/// A <see cref="ICommand{TAuthenticationToken}"/> that requests an existing order gets updateds.
	/// </summary>
	public class UpdateOrderCommand : ICommand<string>
	{
		#region Implementation of ICommand

		[DataMember]
		public Guid Id
		{
			get { return Rsn; }
			set { Rsn = value; }
		}

		[DataMember]
		public int ExpectedVersion { get; set; }

		#endregion

		#region Implementation of IMessageWithAuthenticationToken<string>

		[DataMember]
		public string AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IMessage

		[DataMember]
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

		public UpdateOrderCommand(Guid rsn, int orderId, string customerId, int? employeeId, DateTime? orderDate, DateTime? requiredDate, DateTime? shippedDate, int? shipViaId, decimal? freight, string shipName, string shipAddress, string shipCity, string shipRegion, string shipPostalCode, string shipCountry)
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

		/// <summary>
		/// Explicit cast of <see cref="OrderViewModel"/> to <see cref="UpdateOrderCommand"/>
		/// </summary>
		/// <param name="order">A <see cref="OrderViewModel"/> <see cref="Entity"/> to convert</param>
		/// <returns>A <see cref="CreateOrderCommand"/> object</returns>
		public static explicit operator UpdateOrderCommand(OrderViewModel order)
		{
			var result = new UpdateOrderCommand
			(
				order.Rsn,
				order.OrderID,
				order.CustomerID,
				order.EmployeeID,
				order.OrderDate,
				null,
				order.ShippedDate,
				order.ShipVia,
				order.Freight,
				order.ShipName,
				order.ShipAddress,
				order.ShipCity,
				null,
				order.ShipPostalCode,
				order.ShipCountry
			);

			return result;
		}
	}
}