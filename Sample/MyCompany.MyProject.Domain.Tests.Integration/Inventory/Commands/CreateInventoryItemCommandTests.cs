using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Authentication;
using Cqrs.Commands;
using Cqrs.DataStores;
using Cqrs.Logging;
using Cqrs.Ninject.Configuration;
using Cqrs.Ninject.InProcess.CommandBus.Configuration;
using Cqrs.Ninject.InProcess.EventBus.Configuration;
using Cqrs.Ninject.InProcess.EventStore.Configuration;
using MyCompany.MyProject.Domain.Configuration;
using MyCompany.MyProject.Domain.Factories;
using MyCompany.MyProject.Domain.Inventory.Commands;
using MyCompany.MyProject.Domain.Inventory.Entities;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;

namespace MyCompany.MyProject.Domain.Tests.Integration.Inventory.Commands
{
	/// <summary>
	/// A series of tests on the <see cref="CreateInventoryItemCommand"/> class
	/// </summary>
	[TestClass]
	public class CreateInventoryItemCommandTests : WiredUpTests
	{
		[TestMethod]
		public void CommandBusSend_NonExistingUser_NewInventoryItemEntityCreated()
		{
			// Arrange
			var domainDataStoreFactory = NinjectDependencyResolver.Current.Resolve<IDomainDataStoreFactory>();
			domainDataStoreFactory.GetInventoryItemDataStore().RemoveAll();
			var commandBus = NinjectDependencyResolver.Current.Resolve<ICommandSender<ISingleSignOnToken>>();

			var command = new CreateInventoryItemCommand(Guid.NewGuid(), "New Inventory Item");

			// Act
			DateTime start = DateTime.Now;
			commandBus.Send(command);
			DateTime end = DateTime.Now;

			// Assert
			IDataStore<InventoryItemEntity> dataStore = domainDataStoreFactory.GetInventoryItemDataStore();
			IEnumerable<InventoryItemEntity> query = dataStore.Where(inventoryItem => !inventoryItem.IsLogicallyDeleted && inventoryItem.Rsn == command.Rsn)
				.AsEnumerable();
			InventoryItemEntity result = query.Single();
			Assert.AreEqual(command.Rsn, result.Rsn);
			Assert.AreEqual(command.Name, result.Name);
			Console.WriteLine("Operation took: {0}", end - start);
		}

		[TestMethod]
		public void CommandBusSend_NonExistingUser_NewInventoryItemSummaryEntityCreated()
		{
			// Arrange
			var domainDataStoreFactory = NinjectDependencyResolver.Current.Resolve<IDomainDataStoreFactory>();
			domainDataStoreFactory.GetInventoryItemSummaryDataStore().RemoveAll();
			var commandBus = NinjectDependencyResolver.Current.Resolve<ICommandSender<ISingleSignOnToken>>();

			var command = new CreateInventoryItemCommand(Guid.NewGuid(), "New Inventory Item");

			// Act
			DateTime start = DateTime.Now;
			commandBus.Send(command);
			DateTime end = DateTime.Now;

			// Assert
			IDataStore<InventoryItemSummaryEntity> dataStore = domainDataStoreFactory.GetInventoryItemSummaryDataStore();
			IEnumerable<InventoryItemSummaryEntity> query = dataStore.Where(inventoryItem => !inventoryItem.IsLogicallyDeleted && inventoryItem.Rsn == command.Rsn)
				.AsEnumerable();
			InventoryItemSummaryEntity result = query.Single();
			Assert.AreEqual(command.Rsn, result.Rsn);
			Assert.AreEqual(command.Name, result.Name);
			Console.WriteLine("Operation took: {0}", end - start);
		}

		[TestMethod, ExpectedException(typeof(Cqrs.Domain.Exception.ConcurrencyException))]
		public void CommandBusSend_SendCommandTwice_ConcurrencyExceptionIsRaised()
		{
			// Arrange
			var commandBus = NinjectDependencyResolver.Current.Resolve<ICommandSender<ISingleSignOnToken>>();

			var command = new CreateInventoryItemCommand(Guid.NewGuid(), "New Inventory Item");

			// Act
			DateTime start = DateTime.Now;
			commandBus.Send(command);
			DateTime end = DateTime.Now;

			DateTime start1 = DateTime.Now;
			try
			{
				commandBus.Send(command);
			}
			finally
			{
				DateTime end1 = DateTime.Now;
				Console.WriteLine("Operation took: {0}", end - start);
				Console.WriteLine("Operation second run took: {0}", end1 - start1);
			}
		}
	}
}