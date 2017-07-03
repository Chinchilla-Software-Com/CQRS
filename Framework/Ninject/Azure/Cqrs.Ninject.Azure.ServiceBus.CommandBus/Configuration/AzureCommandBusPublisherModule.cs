#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Linq;
using Cqrs.Azure.ServiceBus;
using Cqrs.Commands;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.ServiceBus.CommandBus.Configuration
{
	/// <summary>
	/// The <see cref="INinjectModule"/> for use with the Cqrs package.
	/// </summary>
	public class AzureCommandBusPublisherModule<TAuthenticationToken> : NinjectModule
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
		/// Register the Cqrs command sender
		/// </summary>
		public virtual void RegisterCommandSender()
		{
			Bind<ICommandSender<TAuthenticationToken>>()
				.To<AzureCommandBusPublisher<TAuthenticationToken>>()
				.InSingletonScope();

			Bind<ICommandPublisher<TAuthenticationToken>>()
				.To<AzureCommandBusPublisher<TAuthenticationToken>>()
				.InSingletonScope();

			Bind<ISendAndWaitCommandSender<TAuthenticationToken>>()
				.To<AzureCommandBusPublisher<TAuthenticationToken>>()
				.InSingletonScope();
		}

		/// <summary>
		/// Register the Cqrs command handler message serialiser
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