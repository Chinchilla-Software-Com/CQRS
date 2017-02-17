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
using Cqrs.Repositories.Queries;
using Ninject.Modules;
using Ninject.Parameters;

namespace Cqrs.Ninject.Configuration
{
	/// <summary>
	/// The <see cref="INinjectModule"/> for use with the Cqrs package.
	/// </summary>
	public class CqrsModule<TAuthenticationToken, TAuthenticationTokenHelper> : NinjectModule
		where TAuthenticationTokenHelper : class, IAuthenticationTokenHelper<TAuthenticationToken>
	{
		protected bool SetupForWeb { get; private set; }

		protected bool SetupForSqlLogging { get; private set; }

		/// <param name="setupForWeb">Set this to true if you will host this in IIS or some other web-server that provides access to System.Web.HttpContext.Current</param>
		/// <param name="setupForSqlLogging">Set this to true to use <see cref="SqlLogger"/> otherwise the <see cref="ConsoleLogger"/> will be bootstrapped by default.</param>
		public CqrsModule(bool setupForWeb, bool setupForSqlLogging)
		{
			SetupForWeb = setupForWeb;
			SetupForSqlLogging = setupForSqlLogging;
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
			Bind<ICorrelationIdHelper>()
				.To<WebCorrelationIdHelper>()
				.InSingletonScope();

			if (SetupForSqlLogging)
				Bind<ILogger>()
					.To<SqlLogger>()
					.InSingletonScope();
			else
				Bind<ILogger>()
					.To<ConsoleLogger>()
					.InSingletonScope();

			Bind<ILoggerSettings>()
				.To<LoggerSettings>()
				.InSingletonScope();
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
			if (SetupForWeb)
			{
				Bind<IContextItemCollectionFactory>()
					.To<WebContextItemCollectionFactory>()
					.InSingletonScope();
			}
			else
			{
				Bind<IContextItemCollectionFactory>()
					.To<ThreadedContextItemCollectionFactory>()
					.InSingletonScope();
			}
			Bind<IContextItemCollection>()
				.To<WebContextItemCollection>()
				.InSingletonScope();
		}

		/// <summary>
		/// Register the all Cqrs requirements
		/// </summary>
		public virtual void RegisterCqrsRequirements()
		{
			Bind<IUnitOfWork<TAuthenticationToken>>()
				.To<UnitOfWork<TAuthenticationToken>>()
				.InTransientScope();
			Bind<IRepository<TAuthenticationToken>>()
				.To<Repository<TAuthenticationToken>>()
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
		}

		protected T Resolve<T>()
		{
			return (T)Resolve(typeof(T));
		}

		protected object Resolve(Type serviceType)
		{
			return Kernel.Resolve(Kernel.CreateRequest(serviceType, null, new Parameter[0], true, true)).SingleOrDefault();
		}
	}
}