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
using Cqrs.Ninject.Configuration;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.ServiceBus.CommandBus.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="AzureCommandBusPublisher{TAuthenticationToken}"/> as the <see cref="ICommandPublisher{TAuthenticationToken}"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureCommandBusPublisherModule<TAuthenticationToken>
		: ResolvableModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			bool isAzureBusHelper = IsRegistered<IAzureBusHelper<TAuthenticationToken>>();
			if (!isAzureBusHelper)
			{
				Bind<IAzureBusHelper<TAuthenticationToken>>()
					.To<AzureBusHelper<TAuthenticationToken>>()
					.InSingletonScope();
			}

			RegisterCommandSender();
			RegisterCommandMessageSerialiser();
		}

		#endregion

		/// <summary>
		/// Register the CQRS command publisher
		/// </summary>
		public virtual void RegisterCommandSender()
		{
			Bind<
#if NETSTANDARD || NET6_0
				IAsyncCommandPublisher
#else
				ICommandPublisher
#endif
				<TAuthenticationToken>>()
				.To<AzureCommandBusPublisher<TAuthenticationToken>>()
				.InSingletonScope();

			Bind<
#if NETSTANDARD || NET6_0
				IAsyncPublishAndWaitCommandPublisher
#else
				IPublishAndWaitCommandPublisher
#endif
				<TAuthenticationToken>>()
				.To<AzureCommandBusPublisher<TAuthenticationToken>>()
				.InSingletonScope();
		}

		/// <summary>
		/// Register the CQRS command handler message serialiser
		/// </summary>
		public virtual void RegisterCommandMessageSerialiser()
		{
			bool isMessageSerialiserBound = IsRegistered<IMessageSerialiser<TAuthenticationToken>>();
			if (!isMessageSerialiserBound)
			{
				Bind<IMessageSerialiser<TAuthenticationToken>>()
					.To<MessageSerialiser<TAuthenticationToken>>()
					.InSingletonScope();
			}
		}
	}
}