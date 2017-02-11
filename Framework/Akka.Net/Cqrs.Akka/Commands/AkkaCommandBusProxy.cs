#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Akka.Actor;
using Cqrs.Commands;
using Cqrs.Configuration;

namespace Cqrs.Akka.Commands
{
	/// <summary>
	/// A <see cref="ICommandSender{TAuthenticationToken}"/> that proxies <see cref="ICommand{TAuthenticationToken}"/> to the <see cref="IActorRef"/> which acts as a single point of all handler resolutions.
	/// </summary>
	public class AkkaCommandBusProxy<TAuthenticationToken>
		: IAkkaCommandSenderProxy<TAuthenticationToken>
	{
		public AkkaCommandBusProxy(IDependencyResolver dependencyResolver)
		{
			CommandHandlerResolver = (IActorRef)dependencyResolver.Resolve(typeof(BusActor));
		}

		protected IActorRef CommandHandlerResolver { get; private set; }

		#region Implementation of ICommandSender<TAuthenticationToken>

		public void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			bool result = CommandHandlerResolver.Ask<bool>(command).Result;
		}

		#endregion

		public class BusActor
			: ReceiveActor
		{
			public BusActor(IAkkaCommandSender<TAuthenticationToken> commandHandlerResolver)
			{
				CommandHandlerResolver = commandHandlerResolver;
				Receive<ICommand<TAuthenticationToken>>(command => ExecuteReceive(command));
			}

			protected IAkkaCommandSender<TAuthenticationToken> CommandHandlerResolver { get; private set; }

			protected virtual void ExecuteReceive(ICommand<TAuthenticationToken> command)
			{
				try
				{
					CommandHandlerResolver.Send(command);

					Sender.Tell(true);
				}
				catch
				{
					Sender.Tell(false);
					throw;
				}
			}
		}
	}
}