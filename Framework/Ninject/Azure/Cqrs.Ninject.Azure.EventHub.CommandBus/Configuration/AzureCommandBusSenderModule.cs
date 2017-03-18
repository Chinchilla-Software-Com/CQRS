using System.Linq;
using Cqrs.Azure.ServiceBus;
using Cqrs.Commands;
using Ninject.Modules;

namespace Cqrs.Azure.EventHub.CommandBus.Configuration
{
	/// <summary>
	/// The <see cref="INinjectModule"/> for use with the Cqrs package.
	/// </summary>
	public class AzureCommandBusSenderModule<TAuthenticationToken> : NinjectModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			RegisterCommandSender();
			RegisterCommandMessageSerialiser();

			Bind<IAzureBusHelper<TAuthenticationToken>>()
				.To<AzureBusHelper<TAuthenticationToken>>()
				.InSingletonScope();
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
