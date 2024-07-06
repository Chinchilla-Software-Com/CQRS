using Cqrs.Authentication;
using Cqrs.Azure.ConfigurationManager;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.DependencyInjection.Modules;
using Cqrs.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Win32;

namespace Cqrs.DependencyInjection.Tests.Unit
{
	[TestClass]
	public class SimplifiedStartUpTests
	{
		[TestMethod]
		public void DefaultCqrsModule_GetBusHelper_BusHelperIsRegisteredAsSignleton()
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
			Cqrs.Configuration.DependencyResolver.ConfigurationManager = configurationManager;

			var services = new ServiceCollection();

			var obj = new TestSimplifiedStartUp<TestCqrsModule>(configurationManager);
			obj.SetupModulesToLoad(services);

			// Act
			obj.TestStartResolver(services);

			// Assert
			var busHelper1 = DependencyResolver.Current.Resolve<IBusHelper>();
			var busHelper2 = DependencyResolver.Current.Resolve<IBusHelper>();
			Assert.AreEqual(busHelper1, busHelper2);
		}

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
				new InProcessCommandBusModule<Guid>(),
				new InProcessEventBusModule<Guid>(),
				new InProcessEventStoreModule<Guid>()
			};

			foreach (Module supplementaryModule in modules)
				DependencyResolver.ModulesToLoad.Add(supplementaryModule);

			// Act
			DependencyResolver.Start(services, prepareProvidedKernel: true);

			var provider = services.BuildServiceProvider();
			((DependencyResolver)Configuration.DependencyResolver.Current).SetKernel(provider);
		}

		[TestMethod]
		public void DefaultCqrsModules_GetIUnitOfWork_IUnitOfWorkIsRegisteredAsTransient()
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

		[TestMethod]
		public void DefaultCqrsModule_GetUnregisteredClass_BusHelperIsRegisteredAsSignleton()
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
			Cqrs.Configuration.DependencyResolver.ConfigurationManager = configurationManager;

			var services = new ServiceCollection();

			var obj = new TestSimplifiedStartUp<TestCqrsModule>(configurationManager);
			obj.SetupModulesToLoad(services);

			// Act
			obj.TestStartResolver(services);

			// Assert
			var testClass1 = DependencyResolver.Current.Resolve<TestCommandHandler>();
			var testClass2 = DependencyResolver.Current.Resolve<TestCommandHandler>();
			Assert.AreEqual(testClass1.BusHelper, testClass2.BusHelper);
		}

		[TestMethod]
		public void BusRegistrar_RegisterHandlerWithDependencyResolverImplementation_BusHelperIsRegisteredAsSignletonWhenHandlerResolved()
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
			Cqrs.Configuration.DependencyResolver.ConfigurationManager = configurationManager;

			var services = new ServiceCollection();

			var obj = new TestSimplifiedStartUp<TestCqrsModule>(configurationManager);
			obj.SetupModulesToLoad(services);
			obj.TestStartResolver(services);
			var registrar = new BusRegistrar(DependencyResolver.Current);

			// Act
			registrar.Register(typeof(TestCommandHandler));

			// Assert
			var testClass1 = DependencyResolver.Current.Resolve<TestCommandHandler>();
			var testClass2 = DependencyResolver.Current.Resolve<TestCommandHandler>();
			Assert.AreEqual(testClass1.BusHelper, testClass2.BusHelper);
		}

		class TestSimplifiedStartUp<THostModule>
			: SimplifiedStartUp<THostModule>
			where THostModule : Module, new()
		{
			public TestSimplifiedStartUp(IConfigurationManager configurationManager)
				: base(configurationManager)
			{
			}

			public override void SetupModulesToLoad(IServiceCollection services)
			{
				base.SetupModulesToLoad(services);
				DependencyResolver.ModulesToLoad.RemoveAt(1);
				// manually set to not be for web
				DependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<Guid, DefaultAuthenticationTokenHelper>(false, false));
			}

			/// <summary>
			/// Calls <see cref="DependencyResolver.Start"/>
			/// </summary>
			public virtual void TestStartResolver(IServiceCollection kernel)
			{
				StartResolver(kernel);

				((DependencyResolver)Cqrs.Configuration.DependencyResolver.Current).SetKernel(kernel.BuildServiceProvider());
			}
		}

		class TestCqrsModule
			: ResolvableModule
		{
			public override void Load(IServiceCollection services)
			{
				services.AddSingleton(Cqrs.Configuration.DependencyResolver.ConfigurationManager);
				services.AddSingleton<IAsyncCommandHandlerRegistrar, InProcessBus<Guid>>();
			}
		}

		class TestCommandHandler
			: ICommandHandler<Guid, TestCommand>
		{
			public IBusHelper BusHelper { get; set; }

			public TestCommandHandler(IBusHelper busHelper)
			{
				BusHelper = busHelper;
			}

			public async Task HandleAsync(TestCommand message)
			{
				await Task.CompletedTask;
			}
		}

		class TestCommand
			: ICommand<Guid>
		{
			public Guid Id { get; set; }
			public int ExpectedVersion { get; set; }
			public Guid AuthenticationToken { get; set; }
			public Guid CorrelationId { get; set; }
			public string? OriginatingFramework { get; set; }
			public IEnumerable<string>? Frameworks { get; set; }
		}
	}
}