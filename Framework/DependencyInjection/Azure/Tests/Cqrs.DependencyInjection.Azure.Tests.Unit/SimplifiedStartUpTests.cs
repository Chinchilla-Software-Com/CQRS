using System;
using System.Collections.Generic;

using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.DependencyInjection.Azure.ServiceBus.CommandBus;
using Cqrs.DependencyInjection.Azure.ServiceBus.EventBus;
using Cqrs.DependencyInjection.Modules;
using Cqrs.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#if NET472_OR_GREATER
#else
using Cqrs.Azure.ConfigurationManager;
using Microsoft.Extensions.Configuration;
#endif

namespace Cqrs.DependencyInjection.Tests.Unit
{
	[TestClass]
	public class SimplifiedStartUpTests
	{
		protected void SetupDefaultCqrsModulesTest()
		{
			//Arrange
			IConfigurationManager configurationManager;
#if NET472_OR_GREATER
			configurationManager = new Configuration.ConfigurationManager();
#else
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("cqrs.settings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();

			configurationManager = new CloudConfigurationManager(config);
#endif
			Configuration.DependencyResolver.ConfigurationManager = configurationManager;

			var services = new ServiceCollection();

			var modules = new List<Module>
			{
				new CqrsModule<Guid, DefaultAuthenticationTokenHelper>(Configuration.DependencyResolver.ConfigurationManager),
				new SimplifiedSqlModule<Guid>(),
				new AzureCommandBusPublisherModule<Guid>(),
				new AzureCommandBusReceiverModule<Guid>(),
				new AzureEventBusPublisherModule<Guid>(),
				new AzureEventBusReceiverModule<Guid>(),
			};

			foreach (Module supplementaryModule in modules)
				DependencyResolver.ModulesToLoad.Add(supplementaryModule);

			// Act
			DependencyResolver.Start(services, prepareProvidedKernel: true);

			var provider = services.BuildServiceProvider();
			((DependencyResolver)Configuration.DependencyResolver.Current).SetKernel(provider);
		}

		[TestMethod]
		public void DefaultAzureServiceBusModules_GetIUnitOfWork_IUnitOfWorkIsRegisteredAsTransient()
		{
			SetupDefaultCqrsModulesTest();

			// Assert
			var class1 = DependencyResolver.Current.Resolve<IUnitOfWork<Guid>>();
			var class2 = DependencyResolver.Current.Resolve<IUnitOfWork<Guid>>();
			Assert.AreNotEqual(class1, class2);
		}

		[TestMethod]
		public void DefaultCqrsModule_GetISagaUnitOfWork_ISagaUnitOfWorkIsRegisteredAsTransient()
		{
			SetupDefaultCqrsModulesTest();

			// Assert
			var class1 = DependencyResolver.Current.Resolve<ISagaUnitOfWork<Guid>>();
			var class2 = DependencyResolver.Current.Resolve<ISagaUnitOfWork<Guid>>();
			Assert.AreNotEqual(class1, class2);
		}

		[TestMethod]
		public void DefaultCqrsModule_GetIAggregateRepository_IAggregateRepositoryIsRegisteredAsSignleton()
		{
			SetupDefaultCqrsModulesTest();

			// Assert
			var class1 = DependencyResolver.Current.Resolve<IAggregateRepository<Guid>>();
			var class2 = DependencyResolver.Current.Resolve<IAggregateRepository<Guid>>();
			Assert.AreEqual(class1, class2);
		}

		[TestMethod]
		public void DefaultCqrsModule_GetISnapshotAggregateRepository_ISnapshotAggregateRepositoryIsRegisteredAsSignleton()
		{
			SetupDefaultCqrsModulesTest();

			// Assert
			var class1 = DependencyResolver.Current.Resolve<ISnapshotAggregateRepository<Guid>>();
			var class2 = DependencyResolver.Current.Resolve<ISnapshotAggregateRepository<Guid>>();
			Assert.AreEqual(class1, class2);
		}

		[TestMethod]
		public void DefaultCqrsModule_GetISagaRepository_ISagaRepositoryIsRegisteredAsSignleton()
		{
			SetupDefaultCqrsModulesTest();

			// Assert
			var class1 = DependencyResolver.Current.Resolve<ISagaRepository<Guid>>();
			var class2 = DependencyResolver.Current.Resolve<ISagaRepository<Guid>>();
			Assert.AreEqual(class1, class2);
		}
	}
}