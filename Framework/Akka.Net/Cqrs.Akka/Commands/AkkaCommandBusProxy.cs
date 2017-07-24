#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using Akka.Actor;
using cdmdotnet.Logging;
using Cqrs.Akka.Domain;
using Cqrs.Authentication;
using Cqrs.Commands;
using Cqrs.Configuration;

namespace Cqrs.Akka.Commands
{
	/// <summary>
	/// A <see cref="ICommandPublisher{TAuthenticationToken}"/> that proxies <see cref="ICommand{TAuthenticationToken}"/> to the <see cref="IActorRef"/> which acts as a single point of all handler resolutions.
	/// </summary>
	public class AkkaCommandBusProxy<TAuthenticationToken>
		: IAkkaCommandPublisherProxy<TAuthenticationToken>
	{
		public AkkaCommandBusProxy(IDependencyResolver dependencyResolver, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
		{
			CorrelationIdHelper = correlationIdHelper;
			AuthenticationTokenHelper = authenticationTokenHelper;
			CommandHandlerResolver = ((IAkkaAggregateResolver)dependencyResolver).ResolveActor<BusActor>();
		}

		protected IActorRef CommandHandlerResolver { get; private set; }

		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		#region Implementation of ICommandSender<TAuthenticationToken>

		public virtual void Publish<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			// We only set these two properties as they are not going to be available across the thread/task
			if (command.AuthenticationToken == null || command.AuthenticationToken.Equals(default(TAuthenticationToken)))
				command.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();
			command.CorrelationId = CorrelationIdHelper.GetCorrelationId();

			bool result = CommandHandlerResolver.Ask<bool>(command).Result;
		}

		public virtual void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			Publish(command);
		}

		public virtual void Publish<TCommand>(IEnumerable<TCommand> commands)
			where TCommand : ICommand<TAuthenticationToken>
		{
			foreach (TCommand rawCommand in commands)
			{
				// Lambda scoping thing
				TCommand command = rawCommand;
				// We only set these two properties as they are not going to be available across the thread/task
				if (command.AuthenticationToken == null || command.AuthenticationToken.Equals(default(TAuthenticationToken)))
					command.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();
				command.CorrelationId = CorrelationIdHelper.GetCorrelationId();

				bool result = CommandHandlerResolver.Ask<bool>(command).Result;
			}
		}

		public virtual void Send<TCommand>(IEnumerable<TCommand> commands)
			where TCommand : ICommand<TAuthenticationToken>
		{
			Publish(commands);
		}

		#endregion

		public class BusActor
			: ReceiveActor
		{
			public BusActor(IAkkaCommandPublisher<TAuthenticationToken> commandHandlerResolver, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
			{
				CommandHandlerResolver = commandHandlerResolver;
				CorrelationIdHelper = correlationIdHelper;
				AuthenticationTokenHelper = authenticationTokenHelper;
				Receive<ICommand<TAuthenticationToken>>(command => ExecuteReceive(command));
			}

			protected IAkkaCommandPublisher<TAuthenticationToken> CommandHandlerResolver { get; private set; }

			protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

			protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

			protected virtual void ExecuteReceive(ICommand<TAuthenticationToken> command)
			{
				try
				{
					AuthenticationTokenHelper.SetAuthenticationToken(command.AuthenticationToken);
					CorrelationIdHelper.SetCorrelationId(command.CorrelationId);
					CommandHandlerResolver.Publish(command);

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