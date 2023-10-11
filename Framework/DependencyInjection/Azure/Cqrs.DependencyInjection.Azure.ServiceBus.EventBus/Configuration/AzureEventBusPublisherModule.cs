#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Azure.ServiceBus;
using Cqrs.DependencyInjection.Modules;
using Cqrs.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Cqrs.DependencyInjection.Azure.ServiceBus.EventBus
{
	/// <summary>
	/// A <see cref="Module"/> that wires up <see cref="AzureEventBusPublisher{TAuthenticationToken}"/> as the <see cref="IEventPublisher{TAuthenticationToken}"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureEventBusPublisherModule<TAuthenticationToken>
		: ResolvableModule
	{
		#region Overrides of ResolvableModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load(IServiceCollection services)
		{
			bool isAzureBusHelper = IsRegistered<IAzureBusHelper<TAuthenticationToken>>(services);
			if (!isAzureBusHelper)
			{
				services.AddSingleton<IAzureBusHelper<TAuthenticationToken>, AzureBusHelper<TAuthenticationToken>>();
			}

			RegisterEventPublisher(services);
			RegisterEventMessageSerialiser(services);
		}

		#endregion

		/// <summary>
		/// Register the CQRS event publisher
		/// </summary>
		public virtual void RegisterEventPublisher(IServiceCollection services)
		{
			services.AddSingleton<
#if NETSTANDARD
				IAsyncEventPublisher
#else
				IEventPublisher
#endif
				<TAuthenticationToken>, AzureEventBusPublisher<TAuthenticationToken>>();
		}

		/// <summary>
		/// Register the CQRS event handler message serialiser
		/// </summary>
		public virtual void RegisterEventMessageSerialiser(IServiceCollection services)
		{
			bool isMessageSerialiserBound = IsRegistered<IMessageSerialiser<TAuthenticationToken>>(services);
			if (!isMessageSerialiserBound)
			{
				services.AddSingleton<IMessageSerialiser<TAuthenticationToken>, MessageSerialiser<TAuthenticationToken>>();
			}
		}
	}
}
