namespace KendoUI.Northwind.Dashboard.Code
{
	using System;
	using cdmdotnet.Logging;
	using Cqrs.Configuration;
	using Cqrs.Domain;
	using Events;

	public class Order : AggregateRoot<string>
	{
		protected Guid Rsn
		{
			get { return Id; }
			private set { Id = value; }
		}

		protected IDependencyResolver DependencyResolver { get; private set; }

		protected ILogger Logger { get; private set; }

		/// <summary>
		/// A constructor for the <see cref="Cqrs.Domain.Factories.IAggregateFactory"/>.
		/// </summary>
		private Order()
		{
		}

		/// <summary>
		/// A constructor for the <see cref="Cqrs.Domain.Factories.IAggregateFactory"/> to operate with new instances.
		/// </summary>
		private Order(IDependencyResolver dependencyResolver, ILogger logger)
			: this()
		{
			DependencyResolver = dependencyResolver;
			Logger = logger;
		}

		/// <summary>
		/// A constructor for the <see cref="Cqrs.Domain.Factories.IAggregateFactory"/> to operate with existing instances.
		/// </summary>
		public Order(IDependencyResolver dependencyResolver, ILogger logger, Guid rsn)
			: this (dependencyResolver, logger)
		{
			Rsn = rsn;
		}

		/// <summary>
		/// Create a new Order
		/// </summary>
		public virtual void CreateOrder(int orderId, string customerId, int? employeeId, DateTime? orderDate, DateTime? requiredDate, DateTime? shippedDate, int? shipViaId, decimal? freight, string shipName, string shipAddress, string shipCity, string shipRegion, string shipPostalCode, string shipCountry)
		{
			ApplyChange(new OrderCreated(Rsn, orderId, customerId, employeeId, orderDate, requiredDate, shippedDate, shipViaId, freight, shipName, shipAddress, shipCity, shipRegion, shipPostalCode, shipCountry));
		}

		/// <summary>
		/// Update an existing Order
		/// </summary>
		public virtual void UpdateOrder(int orderId, string customerId, int? employeeId, DateTime? orderDate, DateTime? requiredDate, DateTime? shippedDate, int? shipViaId, decimal? freight, string shipName, string shipAddress, string shipCity, string shipRegion, string shipPostalCode, string shipCountry)
		{
			ApplyChange(new OrderUpdated(Rsn, orderId, customerId, employeeId, orderDate, requiredDate, shippedDate, shipViaId, freight, shipName, shipAddress, shipCity, shipRegion, shipPostalCode, shipCountry));
		}

		/// <summary>
		/// Logically delete an existing Order
		/// </summary>
		public virtual void DeleteOrder()
		{
			ApplyChange(new OrderDeleted(Rsn));
		}
	}
}