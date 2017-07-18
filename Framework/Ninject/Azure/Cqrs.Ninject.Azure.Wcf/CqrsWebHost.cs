#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using Cqrs.Authentication;
using Cqrs.Azure.ConfigurationManager;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Ninject.Azure.ServiceBus.CommandBus.Configuration;
using Cqrs.Ninject.Azure.ServiceBus.EventBus.Configuration;
using Cqrs.Ninject.Azure.Wcf.Configuration;
using Cqrs.Ninject.Configuration;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.Wcf
{
	/// <summary>
	/// Execute command and event handlers in a WCF Host using Ninject
	/// </summary>
	public class CqrsWebHost<TAuthenticationToken, TAuthenticationTokenHelper> : TelemetryCoreHost<TAuthenticationToken>
		where TAuthenticationTokenHelper : class, IAuthenticationTokenHelper<TAuthenticationToken>
	{
		#region Overrides of CoreHost

		/// <summary>
		/// Configure the <see cref="DependencyResolver"/>.
		/// </summary>
		protected override void ConfigureDefaultDependencyResolver()
		{
			foreach (INinjectModule supplementaryModule in GetSupplementaryModules())
				NinjectDependencyResolver.ModulesToLoad.Add(supplementaryModule);

			NinjectDependencyResolver.Start(prepareProvidedKernel: true);
		}

		#endregion

		/// <summary>
		/// A collection of <see cref="INinjectModule"/> that are required to be loaded
		/// </summary>
		protected virtual IEnumerable<INinjectModule> GetSupplementaryModules()
		{
			var results = new List<INinjectModule>
			{
				new WebHostModule(),
				new CqrsModule<TAuthenticationToken, TAuthenticationTokenHelper>(true, true)
			};

			results.AddRange(GetCommandBusModules());
			results.AddRange(GetEventBusModules());
			results.AddRange(GetEventStoreModules());

			return results;
		}

		/// <summary>
		/// A collection of <see cref="INinjectModule"/> that configure the Azure Servicebus as a command bus as both
		/// <see cref="ICommandPublisher{TAuthenticationToken}"/> and <see cref="ICommandReceiver{TAuthenticationToken}"/>.
		/// </summary>
		protected virtual IEnumerable<INinjectModule> GetCommandBusModules()
		{
			return new List<INinjectModule>
			{
				new AzureCommandBusReceiverModule<TAuthenticationToken>(),
				new AzureCommandBusPublisherModule<TAuthenticationToken>()
			};
		}

		/// <summary>
		/// A collection of <see cref="INinjectModule"/> that configure the Azure Servicebus as a event bus as both
		/// <see cref="IEventPublisher{TAuthenticationToken}"/> and <see cref="IEventReceiver{TAuthenticationToken}"/>
		/// If the app setting Cqrs.Host.EnableEventReceiving is "false" then no modules will be returned.
		/// </summary>
		protected virtual IEnumerable<INinjectModule> GetEventBusModules()
		{
			return new List<INinjectModule>
			{
				new AzureEventBusReceiverModule<TAuthenticationToken>(),
				new AzureEventBusPublisherModule<TAuthenticationToken>()
			};
		}

		/// <summary>
		/// A collection of <see cref="INinjectModule"/> that configure SQL server as the <see cref="IEventStore{TAuthenticationToken}"/>
		/// </summary>
		protected virtual IEnumerable<INinjectModule> GetEventStoreModules()
		{
			return new List<INinjectModule>
			{
				new SimplifiedSqlModule<TAuthenticationToken>()
			};
		}
	}
}