﻿#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq;
#if NET40
using cdmdotnet.AutoMapper;
#endif
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Chinchilla.Logging;
using Chinchilla.Logging.Configuration;
using Chinchilla.StateManagement;
using Chinchilla.StateManagement.Threaded;
using Chinchilla.StateManagement.Web;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Repositories.Queries;
using Cqrs.Snapshots;
using Ninject.Modules;

namespace Cqrs.Ninject.Configuration
{
	/// <summary>
	/// The main <see cref="INinjectModule"/> for use with the CQRS package that wires up many of the prerequisites for running CQRS.NET.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	/// <typeparam name="TAuthenticationTokenHelper">The <see cref="Type"/> of the authentication token helper.</typeparam>
	public class CqrsModule<TAuthenticationToken, TAuthenticationTokenHelper> : ResolvableModule
		where TAuthenticationTokenHelper : class, IAuthenticationTokenHelper<TAuthenticationToken>
	{
		/// <summary>
		/// Indicates that web based wire-up is required rather than console, WPF or winforms based wire-up.
		/// </summary>
		protected bool SetupForWeb { get; private set; }

		/// <summary>
		/// Indicates that logging should be configured for SQL Server rather than console.
		/// </summary>
		protected bool SetupForSqlLogging { get; private set; }

		/// <summary>
		/// Indicates that the <see cref="ConfigurationManager"/> should be registered automatically.
		/// </summary>
		protected bool RegisterDefaultConfigurationManager { get; private set; }

		/// <summary>
		/// Indicates that the <see cref="ISnapshotStrategy{TAuthenticationToken}"/> should be registered automatically.
		/// </summary>
		protected bool RegisterDefaultSnapshotStrategy { get; private set; }

		/// <summary>
		/// Indicates that the <see cref="ISnapshotAggregateRepository{TAuthenticationToken}"/> should be registered automatically.
		/// </summary>
		protected bool RegisterDefaultSnapshotAggregateRepository { get; private set; }

		/// <summary>
		/// Indicates that the <see cref="ISnapshotBuilder"/> should be registered automatically.
		/// </summary>
		protected bool RegisterDefaultSnapshotBuilder { get; private set; }

		/// <summary>
		/// Instantiate a new instance of the <see cref="CqrsModule{TAuthenticationToken,TAuthenticationTokenHelper}"/> that uses the provided <paramref name="configurationManager"/>
		/// to read the following configuration settings:
		/// "Cqrs.SetupForWeb": If set to true the system will be configured for hosting in IIS or some other web-server that provides access to System.Web.HttpContext.Current.
		/// "Cqrs.SetupForSqlLogging": If set to true the <see cref="SqlLogger"/> will be bootstrapped by default, otherwise the <see cref="ConsoleLogger"/> will be bootstrapped by default.
		/// "Cqrs.RegisterDefaultConfigurationManager": If set true the <see cref="ConfigurationManager"/> will be registered. If you want to use the Azure one leave this as false (the default) and register it yourself.
		/// "Cqrs.RegisterDefaultSnapshotStrategy": If set true the <see cref="DefaultSnapshotStrategy{TAuthenticationToken}"/> will be registered.
		/// "Cqrs.RegisterDefaultSnapshotAggregateRepository": If set true the <see cref="SnapshotRepository{TAuthenticationToken}"/> will be registered.
		/// "Cqrs.RegisterDefaultSnapshotBuilder": If set true the <see cref="DefaultSnapshotBuilder"/> will be registered.
		/// </summary>
		/// <param name="configurationManager">The <see cref="IConfigurationManager"/> to use, if one isn't provided then <see cref="ConfigurationManager"/> is instantiate, used and then disposed.</param>
		public CqrsModule(IConfigurationManager configurationManager = null)
		{
			configurationManager = configurationManager ?? new ConfigurationManager();
			if (configurationManager.TryGetSetting("Cqrs.SetupForWeb", out bool setupForWeb))
				SetupForWeb = setupForWeb;
			if (configurationManager.TryGetSetting("Cqrs.SetupForSqlLogging", out bool setupForSqlLogging))
				SetupForSqlLogging = setupForSqlLogging;
			if (configurationManager.TryGetSetting("Cqrs.RegisterDefaultConfigurationManager", out bool registerDefaultConfigurationManager))
				RegisterDefaultConfigurationManager = registerDefaultConfigurationManager;
			if (configurationManager.TryGetSetting("Cqrs.RegisterDefaultSnapshotAggregateRepository", out bool registerDefaultSnapshotAggregateRepository))
				RegisterDefaultSnapshotAggregateRepository = registerDefaultSnapshotAggregateRepository;
			else
				RegisterDefaultSnapshotAggregateRepository = true;
			if (configurationManager.TryGetSetting("Cqrs.RegisterDefaultSnapshotStrategy", out bool registerDefaultSnapshotStrategy))
				RegisterDefaultSnapshotStrategy = registerDefaultSnapshotStrategy;
			else
				RegisterDefaultSnapshotStrategy = true;
			if (configurationManager.TryGetSetting("Cqrs.RegisterDefaultSnapshotBuilder", out bool registerDefaultSnapshotBuilder))
				RegisterDefaultSnapshotBuilder = registerDefaultSnapshotBuilder;
			else
				RegisterDefaultSnapshotBuilder = true;
		}

		/// <summary>
		/// Instantiate a new instance of the <see cref="CqrsModule{TAuthenticationToken,TAuthenticationTokenHelper}"/>.
		/// </summary>
		/// <param name="setupForWeb">Set this to true if you will host this in IIS or some other web-server that provides access to System.Web.HttpContext.Current.</param>
		/// <param name="setupForSqlLogging">Set this to true to use <see cref="SqlLogger"/> otherwise the <see cref="ConsoleLogger"/> will be bootstrapped by default.</param>
		/// <param name="registerDefaultConfigurationManager">Set this to true to use <see cref="ConfigurationManager"/>. If you want to use the Azure one leave this as false (the default) and register it yourself.</param>
		/// <param name="registerDefaultSnapshotAggregateRepository">If set true the <see cref="SnapshotRepository{TAuthenticationToken}"/> will be registered.</param>
		/// <param name="registerDefaultSnapshotStrategy">If set true the <see cref="DefaultSnapshotStrategy{TAuthenticationToken}"/> will be registered.</param>
		/// <param name="registerDefaultSnapshotBuilder">If set true the <see cref="DefaultSnapshotBuilder"/> will be registered.</param>
		public CqrsModule(bool setupForWeb, bool setupForSqlLogging, bool registerDefaultConfigurationManager = false, bool registerDefaultSnapshotAggregateRepository = true, bool registerDefaultSnapshotStrategy = true, bool registerDefaultSnapshotBuilder = true)
		{
			SetupForWeb = setupForWeb;
			SetupForSqlLogging = setupForSqlLogging;
			RegisterDefaultConfigurationManager = registerDefaultConfigurationManager;
			RegisterDefaultSnapshotAggregateRepository = registerDefaultSnapshotAggregateRepository;
			RegisterDefaultSnapshotStrategy = registerDefaultSnapshotStrategy;
			RegisterDefaultSnapshotBuilder = registerDefaultSnapshotBuilder;
		}

#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			RegisterFactories();
			RegisterRepositories();
			RegisterQueryBuilders();
			RegisterServices();
			RegisterCqrsRequirements();
#if NET40
			RegisterAutomapperComponents();
#endif
			RegisterLoggerComponents();
			RegisterCaching();
		}

#endregion

		/// <summary>
		/// Register the all factories
		/// </summary>
		public virtual void RegisterFactories()
		{
			Bind<IQueryFactory>()
				.To<QueryFactory>()
				.InSingletonScope();
			Bind<IHashAlgorithmFactory>()
				.To<BuiltInHashAlgorithmFactory>()
				.InSingletonScope();
		}

		/// <summary>
		/// Register the all components for the <see cref="ILogger"/>
		/// </summary>
		public virtual void RegisterLoggerComponents()
		{
			bool isCorrelationIdHelperBound = Kernel.GetBindings(typeof(ICorrelationIdHelper)).Any();
			if (!isCorrelationIdHelperBound)
			{
				if (SetupForWeb)
					Bind<ICorrelationIdHelper>()
						.To<WebCorrelationIdHelper>()
						.InSingletonScope();
				else
					Bind<ICorrelationIdHelper>()
						.To<CorrelationIdHelper>()
						.InSingletonScope();
			}

			bool isLoggerBound = Kernel.GetBindings(typeof(ILogger)).Any();
			if (!isLoggerBound)
			{
				if (SetupForSqlLogging)
					Bind<ILogger>()
						.To<SqlLogger>()
						.InSingletonScope();
				else
					Bind<ILogger>()
						.To<ConsoleLogger>()
						.InSingletonScope();
			}

			bool isLoggerSettingsBound = Kernel.GetBindings(typeof(ILoggerSettings)).Any();
			if (!isLoggerSettingsBound)
			{
				Bind<ILoggerSettings>()
					.To<LoggerSettings>()
					.InSingletonScope();
			}

			bool isTelemetryHelperBound = Kernel.GetBindings(typeof(ITelemetryHelper)).Any();
			if (!isTelemetryHelperBound)
			{
				Bind<ITelemetryHelper>()
					.To<NullTelemetryHelper>()
					.InSingletonScope();
			}
		}

#if NET40
		/// <summary>
		/// Register the all <see cref="IAutomapHelper"/>
		/// </summary>
		public virtual void RegisterAutomapperComponents()
		{
			Bind<IAutomapHelper>()
				.To<AutomapHelper>()
				.InSingletonScope();
		}
#endif

		/// <summary>
		/// Register the all repositories
		/// </summary>
		public virtual void RegisterRepositories()
		{
		}

		/// <summary>
		/// Register the all query builders
		/// </summary>
		public virtual void RegisterQueryBuilders()
		{
		}

		/// <summary>
		/// Register the all services
		/// </summary>
		public virtual void RegisterServices()
		{
		}

		/// <summary>
		/// Register the all caching stuffs
		/// </summary>
		public virtual void RegisterCaching()
		{
			if (Kernel.GetBindings(typeof (IContextItemCollectionFactory)).Any())
				Kernel.Unbind<IContextItemCollectionFactory>();
			if (Kernel.GetBindings(typeof(IContextItemCollection)).Any())
				Kernel.Unbind<IContextItemCollection>();
			if (SetupForWeb)
			{
				Bind<IContextItemCollectionFactory>()
					.To<WebContextItemCollectionFactory>()
					.InSingletonScope();
				Bind<IContextItemCollection>()
					.To<WebContextItemCollection>()
					.InSingletonScope();
			}
			else
			{
				Bind<IContextItemCollectionFactory>()
					.To<ContextItemCollectionFactory>()
					.InSingletonScope();
				Bind<IContextItemCollection>()
					.To<Chinchilla.StateManagement.Threaded.ContextItemCollection>()
					.InSingletonScope();
			}
		}

		/// <summary>
		/// Register the all Cqrs requirements
		/// </summary>
		public virtual void RegisterCqrsRequirements()
		{
			Bind<IUnitOfWork<TAuthenticationToken>>()
				.To<UnitOfWork<TAuthenticationToken>>()
				.InTransientScope();
			Bind<ISagaUnitOfWork<TAuthenticationToken>>()
				.To<SagaUnitOfWork<TAuthenticationToken>>()
				.InTransientScope();
			Bind<IAggregateRepository<TAuthenticationToken>>()
				.To<AggregateRepository<TAuthenticationToken>>()
				.InSingletonScope();

			if (RegisterDefaultSnapshotAggregateRepository)
				Bind<ISnapshotAggregateRepository<TAuthenticationToken>>()
					.To<SnapshotRepository<TAuthenticationToken>>()
					.InSingletonScope();

			Bind<ISagaRepository<TAuthenticationToken>>()
				.To<SagaRepository<TAuthenticationToken>>()
				.InSingletonScope();
			Bind<IAggregateFactory>()
				.To<AggregateFactory>()
				.InSingletonScope();

			Bind<IAuthenticationTokenHelper<TAuthenticationToken>>()
				.To<TAuthenticationTokenHelper>()
				.InSingletonScope();

			Bind<IStoreLastEventProcessed>()
				.To<FileBasedLastEventProcessedStore>()
				.InSingletonScope();

			Bind<IBusHelper>()
				.To<BusHelper>()
				.InSingletonScope();

			if (RegisterDefaultConfigurationManager)
				Bind<IConfigurationManager>()
					.To<ConfigurationManager>()
					.InSingletonScope();

			if (RegisterDefaultSnapshotStrategy)
				Bind<ISnapshotStrategy<TAuthenticationToken>>()
					.To<DefaultSnapshotStrategy<TAuthenticationToken>>()
					.InSingletonScope();

			if (RegisterDefaultSnapshotBuilder)
				Bind<ISnapshotBuilder>()
					.To<DefaultSnapshotBuilder>()
					.InSingletonScope();
		}
	}
}