using System.Linq;
using Cqrs.Azure.ServiceBus;
using Cqrs.Bus;
using Cqrs.Events;
using Ninject;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.ServiceBus.EventBus.Configuration
{
	/// <summary>
	/// The <see cref="INinjectModule"/> for use with the Cqrs package.
	/// </summary>
	public class AzureEventBusReceiverModule<TAuthenticationToken> : NinjectModule
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

			RegisterEventMessageSerialiser();
			var bus = GetOrCreateBus<AzureEventBusReceiver<TAuthenticationToken>>();

			RegisterEventReceiver(bus);
			RegisterEventHandlerRegistrar(bus);
		}

		#endregion

		public virtual TBus GetOrCreateBus<TBus>()
			where TBus : IEventReceiver<TAuthenticationToken>, IEventHandlerRegistrar
		{
			bool isBusBound = Kernel.GetBindings(typeof(TBus)).Any();
			TBus bus;
			if (!isBusBound)
			{
				bus = Kernel.Get<TBus>();
				Bind<TBus>()
					.ToConstant(bus)
					.InSingletonScope();
			}
			else
				bus = Kernel.Get<TBus>();

			return bus;
		}

		/// <summary>
		/// Register the Cqrs event receiver
		/// </summary>
		public virtual void RegisterEventReceiver<TBus>(TBus bus)
			where TBus : IEventReceiver<TAuthenticationToken>, IEventHandlerRegistrar
		{
			Bind<IEventReceiver<TAuthenticationToken>>()
				.ToConstant(bus)
				.InSingletonScope();
		}

		/// <summary>
		/// Register the Cqrs event handler registrar
		/// </summary>
		public virtual void RegisterEventHandlerRegistrar<TBus>(TBus bus)
			where TBus : IEventReceiver<TAuthenticationToken>, IEventHandlerRegistrar
		{
			bool isHandlerRegistrationBound = Kernel.GetBindings(typeof(IEventHandlerRegistrar)).Any();
			if (!isHandlerRegistrationBound)
			{
				Bind<IEventHandlerRegistrar>()
					.ToConstant(bus)
					.InSingletonScope();
			}
		}

		/// <summary>
		/// Register the Cqrs event handler message serialiser
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
