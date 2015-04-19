using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Authentication;
using Cqrs.DataStores;
using Cqrs.Ninject.Configuration;
using Cqrs.Services;
using MyCompany.MyProject.Domain.Factories;
using MyCompany.MyProject.Domain.Inventory.Entities;
using MyCompany.MyProject.Domain.Inventory.Services;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;

namespace MyCompany.MyProject.Domain.Tests.Integration.Inventory.Services
{
	/// <summary>
	/// A series of tests on the <see cref="InventoryItemService"/> class
	/// </summary>
	[TestClass]
	public class InventoryItemServiceTests : WiredUpTests
	{
		[TestMethod]
		public void Create_NonExistingUser_NewInventoryItemEntityCreated()
		{
			// Arrange
			var domainDataStoreFactory = NinjectDependencyResolver.Current.Resolve<IDomainDataStoreFactory>();
			domainDataStoreFactory.GetInventoryItemDataStore().RemoveAll();
			var authenticationTokenHelper = NinjectDependencyResolver.Current.Resolve<IAuthenticationTokenHelper<ISingleSignOnToken>>();
			ISingleSignOnToken token = authenticationTokenHelper.GetAuthenticationToken();
			var service = NinjectDependencyResolver.Current.Resolve<IInventoryItemService>();

			var parameters = new ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceCreateParameters>
			{
				AuthenticationToken = token,
				Data = new InventoryItemServiceCreateParameters {name = "New Inventory Item"}
			};

			// Act
			DateTime start = DateTime.Now;
			service.Create(parameters);
			DateTime end = DateTime.Now;

			// Assert
			IDataStore<InventoryItemEntity> dataStore = domainDataStoreFactory.GetInventoryItemDataStore();
			IEnumerable<InventoryItemEntity> query = dataStore.Where(inventoryItem => !inventoryItem.IsLogicallyDeleted && inventoryItem.Name == parameters.Data.name)
				.AsEnumerable();
			InventoryItemEntity result = query.Single();
			Assert.AreEqual(parameters.Data.name, result.Name);
			Console.WriteLine("Operation took: {0}", end - start);
		}

		[TestMethod]
		public void Create_NonExistingUser_NewInventoryItemSummaryEntityCreated()
		{
			// Arrange
			var domainDataStoreFactory = NinjectDependencyResolver.Current.Resolve<IDomainDataStoreFactory>();
			domainDataStoreFactory.GetInventoryItemSummaryDataStore().RemoveAll();
			var authenticationTokenHelper = NinjectDependencyResolver.Current.Resolve<IAuthenticationTokenHelper<ISingleSignOnToken>>();
			ISingleSignOnToken token = authenticationTokenHelper.GetAuthenticationToken();
			var service = NinjectDependencyResolver.Current.Resolve<IInventoryItemService>();

			var parameters = new ServiceRequestWithData<ISingleSignOnToken, InventoryItemServiceCreateParameters>
			{
				AuthenticationToken = token,
				Data = new InventoryItemServiceCreateParameters { name = "New Inventory Item" }
			};

			// Act
			DateTime start = DateTime.Now;
			service.Create(parameters);
			DateTime end = DateTime.Now;

			// Assert
			IDataStore<InventoryItemSummaryEntity> dataStore = domainDataStoreFactory.GetInventoryItemSummaryDataStore();
			IEnumerable<InventoryItemSummaryEntity> query = dataStore.Where(inventoryItem => !inventoryItem.IsLogicallyDeleted && inventoryItem.Name == parameters.Data.name)
				.AsEnumerable();
			InventoryItemSummaryEntity result = query.Single();
			Assert.AreEqual(parameters.Data.name, result.Name);
			Console.WriteLine("Operation took: {0}", end - start);
		}
	}
}