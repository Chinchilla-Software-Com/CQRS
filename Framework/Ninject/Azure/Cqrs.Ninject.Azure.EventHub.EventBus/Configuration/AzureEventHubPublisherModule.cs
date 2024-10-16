﻿#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq;
using Cqrs.Azure.ServiceBus;
using Cqrs.Events;
using Ninject.Modules;

namespace Cqrs.Azure.EventHub.EventBus.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="AzureEventBusPublisher{TAuthenticationToken}"/> as the <see cref="IEventPublisher{TAuthenticationToken}"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureEventHubPublisherModule<TAuthenticationToken> : NinjectModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			bool isMessageSerialiserBound = Kernel.GetBindings(typeof(IAzureBusHelper<TAuthenticationToken>)).Any();
			if (!isMessageSerialiserBound)
			{
				Bind<IAzureBusHelper<TAuthenticationToken>>()
					.To<AzureBusHelper<TAuthenticationToken>>()
					.InSingletonScope();
			}

			RegisterEventPublisher();
			RegisterEventMessageSerialiser();
		}

		#endregion

		/// <summary>
		/// Register the CQRS event publisher
		/// </summary>
		public virtual void RegisterEventPublisher()
		{
#if NETSTANDARD2_0
			Bind<IAsyncEventPublisher<TAuthenticationToken>>()
#else
			Bind<IEventPublisher<TAuthenticationToken>>()
#endif
				.To<AzureEventBusPublisher<TAuthenticationToken>>()
				.InSingletonScope();
		}

		/// <summary>
		/// Register the CQRS event handler message serialiser
		/// </summary>
		public virtual void RegisterEventMessageSerialiser()
		{
			bool isMessageSerialiserBound = Kernel.GetBindings(typeof(IMessageSerialiser<TAuthenticationToken>)).Any();
			if (!isMessageSerialiserBound)
			{
				Bind<IMessageSerialiser<TAuthenticationToken>>()
					.To<MessageSerialiser<TAuthenticationToken>>()
					.InSingletonScope();
			}
		}
	}
}
