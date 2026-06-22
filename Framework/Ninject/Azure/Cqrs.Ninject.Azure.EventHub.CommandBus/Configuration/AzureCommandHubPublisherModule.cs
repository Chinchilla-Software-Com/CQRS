#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq;
using Cqrs.Azure.ServiceBus;
using Cqrs.Commands;
using Ninject.Modules;

namespace Cqrs.Azure.EventHub.CommandBus.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="AzureCommandBusPublisher{TAuthenticationToken}"/> as the <see cref="ICommandPublisher{TAuthenticationToken}"/> and other require components.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AzureCommandHubPublisherModule<TAuthenticationToken> : NinjectModule
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

			RegisterCommandSender();
			RegisterCommandMessageSerialiser();
		}

		#endregion

		/// <summary>
		/// Register the CQRS command publisher
		/// </summary>
		public virtual void RegisterCommandSender()
		{
			Bind<ICommandPublisher<TAuthenticationToken>>()
				.To<AzureCommandBusPublisher<TAuthenticationToken>>()
				.InSingletonScope();

			Bind<ICommandPublisher<TAuthenticationToken>>()
				.To<AzureCommandBusPublisher<TAuthenticationToken>>()
				.InSingletonScope();

			Bind<IPublishAndWaitCommandPublisher<TAuthenticationToken>>()
				.To<AzureCommandBusPublisher<TAuthenticationToken>>()
				.InSingletonScope();
		}

		/// <summary>
		/// Register the CQRS command handler message serialiser
		/// </summary>
		public virtual void RegisterCommandMessageSerialiser()
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
