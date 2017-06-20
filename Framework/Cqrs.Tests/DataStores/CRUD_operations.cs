using System;
using System.Collections.Generic;
using System.Linq;
using cdmdotnet.Logging;
using cdmdotnet.Logging.Configuration;
using cdmdotnet.StateManagement.Threaded;
using Cqrs.Configuration;
using Cqrs.DataStores;
using Cqrs.Tests.Substitutes;
using NUnit.Framework;

namespace Cqrs.Tests.DataStores
{
	[TestFixture]
	public class CRUD_operations
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void GetAllItems()
		{
			// Arrange
			var sqlDataStore = new SqlDataStore<OrderEntity>(new ConfigurationManager(), new ConsoleLogger(new LoggerSettings(), new CorrelationIdHelper(new ThreadedContextItemCollectionFactory())));

			// Act
			var actualData = sqlDataStore.ToList();

			// Assert
			Assert.AreEqual(830, actualData.Count);
		}

		[Test]
		public void AddSingleItem()
		{
			// Arrange
			var sqlDataStore = new SqlDataStore<OrderEntity>(new ConfigurationManager(), new ConsoleLogger(new LoggerSettings(), new CorrelationIdHelper(new ThreadedContextItemCollectionFactory())));
			var entityData = new OrderEntity
			{
				Rsn = Guid.NewGuid(),
				CustomerId = "BOLID",
				EmployeeId = 1,
				Freight = 153.24M,
				OrderDate = DateTime.UtcNow.AddDays(-3),
				RequiredDate = DateTime.UtcNow.AddDays(2),
				ShipAddress = "Shipping Address",
				ShipCity = "Shipping City",
				ShipCountry = "ShippingCountry",
				ShipName = "Persons Name",
				ShipPostalCode = "14287",
				ShipRegion = "Region RT",
				ShipViaId = 3,
				ShippedDate = DateTime.UtcNow.AddDays(5),
				SortingOrder = 1
			};

			// Act
			sqlDataStore.Add(entityData);

			// Assert
			Assert.AreEqual(831, sqlDataStore.ToList().Count);

			// Clean-up
			sqlDataStore.Destroy(entityData);
			Assert.AreEqual(830, sqlDataStore.ToList().Count);
		}

		[Test]
		public void AddTwoItems()
		{
			// Arrange
			var sqlDataStore = new SqlDataStore<OrderEntity>(new ConfigurationManager(), new ConsoleLogger(new LoggerSettings(), new CorrelationIdHelper(new ThreadedContextItemCollectionFactory())));
			var entityData = new List<OrderEntity>
			{
				new OrderEntity
				{
					Rsn = Guid.NewGuid(),
					CustomerId = "BOLID",
					EmployeeId = 1,
					Freight = 153.24M,
					OrderDate = DateTime.UtcNow.AddDays(-3),
					RequiredDate = DateTime.UtcNow.AddDays(2),
					ShipAddress = "Shipping Address",
					ShipCity = "Shipping City",
					ShipCountry = "ShippingCountry",
					ShipName = "Persons Name",
					ShipPostalCode = "14287",
					ShipRegion = "Region RT",
					ShipViaId = 3,
					ShippedDate = DateTime.UtcNow.AddDays(5),
					SortingOrder = 1
				},
								new OrderEntity
				{
					Rsn = Guid.NewGuid(),
					CustomerId = "BOLID",
					EmployeeId = 2,
					Freight = 742.15M,
					OrderDate = DateTime.UtcNow.AddDays(-4),
					RequiredDate = DateTime.UtcNow.AddDays(1),
					ShipAddress = "Shipping Address",
					ShipCity = "Shipping City",
					ShipCountry = "ShippingCountry",
					ShipName = "Persons Name",
					ShipPostalCode = "14287",
					ShipRegion = "Region RT",
					ShipViaId = 2,
					ShippedDate = DateTime.UtcNow.AddDays(9),
					SortingOrder = 2
				}
			};

			// Act
			sqlDataStore.Add(entityData);

			// Assert
			Assert.AreEqual(832, sqlDataStore.ToList().Count);

			// Clean-up
			foreach (OrderEntity orderEntity in entityData)
				sqlDataStore.Destroy(orderEntity);
			Assert.AreEqual(830, sqlDataStore.ToList().Count);
		}

		[Test]
		public void UpdateSingleItem()
		{
			// Arrange
			var sqlDataStore = new SqlDataStore<OrderEntity>(new ConfigurationManager(), new ConsoleLogger(new LoggerSettings(), new CorrelationIdHelper(new ThreadedContextItemCollectionFactory())));
			var entityData = new OrderEntity
			{
				Rsn = Guid.NewGuid(),
				CustomerId = "BOLID",
				EmployeeId = 1,
				Freight = 153.24M,
				OrderDate = DateTime.UtcNow.AddDays(-3),
				RequiredDate = DateTime.UtcNow.AddDays(2),
				ShipAddress = "Shipping Address",
				ShipCity = "Shipping City",
				ShipCountry = "ShippingCountry",
				ShipName = "Persons Name",
				ShipPostalCode = "14287",
				ShipRegion = "Region RT",
				ShipViaId = 3,
				ShippedDate = DateTime.UtcNow.AddDays(5),
				SortingOrder = 1
			};
			sqlDataStore.Add(entityData);
			entityData = sqlDataStore.Single(e => e.Rsn.Equals(entityData.Rsn));
			entityData.CustomerId = "CHOPS";

			// Act
			sqlDataStore.Update(entityData);

			// Assert
			entityData = sqlDataStore.Single(e => e.Rsn.Equals(entityData.Rsn)); ;
			Assert.AreEqual("CHOPS", entityData.CustomerId);

			// Clean-up
			sqlDataStore.Destroy(entityData);
			Assert.AreEqual(830, sqlDataStore.ToList().Count);
		}

		[Test]
		public void RemoveSingleItem()
		{
			// Arrange
			var sqlDataStore = new SqlDataStore<OrderEntity>(new ConfigurationManager(), new ConsoleLogger(new LoggerSettings(), new CorrelationIdHelper(new ThreadedContextItemCollectionFactory())));
			var entityData = new OrderEntity
			{
				Rsn = Guid.NewGuid(),
				CustomerId = "BOLID",
				EmployeeId = 1,
				Freight = 153.24M,
				OrderDate = DateTime.UtcNow.AddDays(-3),
				RequiredDate = DateTime.UtcNow.AddDays(2),
				ShipAddress = "Shipping Address",
				ShipCity = "Shipping City",
				ShipCountry = "ShippingCountry",
				ShipName = "Persons Name",
				ShipPostalCode = "14287",
				ShipRegion = "Region RT",
				ShipViaId = 3,
				ShippedDate = DateTime.UtcNow.AddDays(5),
				SortingOrder = 1
			};
			sqlDataStore.Add(entityData);
			entityData = sqlDataStore.Single(e => e.Rsn.Equals(entityData.Rsn));

			// Act
			sqlDataStore.Remove(entityData);

			// Assert
			entityData = sqlDataStore.Single(e => e.Rsn.Equals(entityData.Rsn)); ;
			Assert.IsTrue(entityData.IsLogicallyDeleted);

			// Clean-up
			sqlDataStore.Destroy(entityData);
			Assert.AreEqual(830, sqlDataStore.ToList().Count);
		}
	}
}