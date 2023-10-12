#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Azure.ServiceBus;
using Cqrs.Commands;
using Cqrs.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace Cqrs.DependencyInjection.Azure.ServiceBus.CommandBus
{
	/// <summary>
	/// A <see cref="Module"/> that wires up <see cref="AzureCommandBusPublisher{TAuthenticationToken}"/> as the <see cref="ICommandPublisher{TAuthenticationToken}"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureCommandBusPublisherModule<TAuthenticationToken>
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
				services.AddSingleton<IAzureBusHelper<TAuthenticationToken>,AzureBusHelper<TAuthenticationToken>>();
			}

			RegisterCommandSender(services);
			RegisterCommandMessageSerialiser(services);
		}

		#endregion

		/// <summary>
		/// Register the CQRS command publisher
		/// </summary>
		public virtual void RegisterCommandSender(IServiceCollection services)
		{
			services.AddSingleton<
#if NETSTANDARD2_0 || NET48_OR_GREATER
				IAsyncCommandPublisher
#else
				ICommandPublisher
#endif
				<TAuthenticationToken>, AzureCommandBusPublisher<TAuthenticationToken>>();

			services.AddSingleton<
#if NETSTANDARD2_0 || NET48_OR_GREATER

				IAsyncPublishAndWaitCommandPublisher
#else
				IPublishAndWaitCommandPublisher
#endif
				<TAuthenticationToken>, AzureCommandBusPublisher<TAuthenticationToken>>();
		}

		/// <summary>
		/// Register the CQRS command handler message serialiser
		/// </summary>
		public virtual void RegisterCommandMessageSerialiser(IServiceCollection services)
		{
			bool isMessageSerialiserBound = IsRegistered<IMessageSerialiser<TAuthenticationToken>>(services);
			if (!isMessageSerialiserBound)
			{
				services.AddSingleton<IMessageSerialiser<TAuthenticationToken>, MessageSerialiser<TAuthenticationToken>>();
			}
		}
	}
}