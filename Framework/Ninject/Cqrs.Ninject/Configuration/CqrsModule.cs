#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq;
using cdmdotnet.AutoMapper;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Domain;
using Cqrs.Domain.Factories;
using cdmdotnet.Logging;
using cdmdotnet.Logging.Configuration;
using cdmdotnet.StateManagement;
using cdmdotnet.StateManagement.Threaded;
using cdmdotnet.StateManagement.Web;
using Cqrs.Configuration;
using Cqrs.Repositories.Queries;
using Ninject.Modules;
using Ninject.Parameters;

namespace Cqrs.Ninject.Configuration
{
	/// <summary>
	/// The <see cref="INinjectModule"/> for use with the Cqrs package.
	/// </summary>
	public class CqrsModule<TAuthenticationToken, TAuthenticationTokenHelper> : ResolvableModule
		where TAuthenticationTokenHelper : class, IAuthenticationTokenHelper<TAuthenticationToken>
	{
		protected bool SetupForWeb { get; private set; }

		protected bool SetupForSqlLogging { get; private set; }

		protected bool RegisterDefaultConfigurationManager { get; private set; }

		/// <param name="setupForWeb">Set this to true if you will host this in IIS or some other web-server that provides access to System.Web.HttpContext.Current</param>
		/// <param name="setupForSqlLogging">Set this to true to use <see cref="SqlLogger"/> otherwise the <see cref="ConsoleLogger"/> will be bootstrapped by default.</param>
		/// <param name="registerDefaultConfigurationManager">Set this to true to use <see cref="ConfigurationManager"/>. If you want to use the Azure one leave this as false (the default) and register it yourself.</param>
		public CqrsModule(bool setupForWeb, bool setupForSqlLogging, bool registerDefaultConfigurationManager = false)
		{
			SetupForWeb = setupForWeb;
			SetupForSqlLogging = setupForSqlLogging;
			RegisterDefaultConfigurationManager = registerDefaultConfigurationManager;
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
			RegisterAutomapperComponents();
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

		/// <summary>
		/// Register the all <see cref="IAutomapHelper"/>
		/// </summary>
		public virtual void RegisterAutomapperComponents()
		{
			Bind<IAutomapHelper>()
				.To<AutomapHelper>()
				.InSingletonScope();
		}

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
					.To<ThreadedContextItemCollectionFactory>()
					.InSingletonScope();
				Bind<IContextItemCollection>()
					.To<ThreadedContextItemCollection>()
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
			Bind<IRepository<TAuthenticationToken>>()
				.To<Repository<TAuthenticationToken>>()
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
		}
	}
}