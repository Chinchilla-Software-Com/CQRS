#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
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
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class AkkaCommandBusProxy<TAuthenticationToken>
		: IAkkaCommandPublisherProxy<TAuthenticationToken>
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="AkkaCommandBusProxy{TAuthenticationToken}"/>.
		/// </summary>
		public AkkaCommandBusProxy(IDependencyResolver dependencyResolver, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
		{
			CorrelationIdHelper = correlationIdHelper;
			AuthenticationTokenHelper = authenticationTokenHelper;
			CommandHandlerResolver = ((IAkkaAggregateResolver)dependencyResolver).ResolveActor<BusActor>();
		}

		/// <summary>
		/// Gets the <see cref="IActorRef">command handler resolver</see> that we send/proxy the command to.
		/// </summary>
		protected IActorRef CommandHandlerResolver { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="ICorrelationIdHelper"/>.
		/// </summary>
		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IAuthenticationTokenHelper{TAuthenticationToken}">Authentication Token Helper</see>.
		/// </summary>
		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		#region Implementation of ICommandSender<TAuthenticationToken>

		/// <summary>
		/// Publishes the provided <paramref name="command"/> on the command bus.
		/// </summary>
		public virtual void Publish<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			// We only set these two properties as they are not going to be available across the thread/task
			if (command.AuthenticationToken == null || command.AuthenticationToken.Equals(default(TAuthenticationToken)))
				command.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();
			command.CorrelationId = CorrelationIdHelper.GetCorrelationId();

			bool result = CommandHandlerResolver.Ask<bool>(command).Result;
		}

		/// <summary>
		/// Publishes the provided <paramref name="commands"/> on the command bus.
		/// </summary>
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

		#endregion

		/// <summary>
		/// Similar to a <see cref="ICommandPublisher{TAuthenticationToken}"/>, passes commands onto the <see cref="CommandHandlerResolver"/>.
		/// </summary>
		public class BusActor
			: ReceiveActor
		{
			/// <summary>
			/// Instantiates a new instance of <see cref="BusActor"/>.
			/// </summary>
			public BusActor(IAkkaCommandPublisher<TAuthenticationToken> commandHandlerResolver, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
			{
				CommandHandlerResolver = commandHandlerResolver;
				CorrelationIdHelper = correlationIdHelper;
				AuthenticationTokenHelper = authenticationTokenHelper;
				Receive<ICommand<TAuthenticationToken>>(command => ExecuteReceive(command));
			}

			/// <summary>
			/// Gets or sets the <see cref="IAkkaCommandPublisher{TAuthenticationToken}"/>.
			/// </summary>
			protected IAkkaCommandPublisher<TAuthenticationToken> CommandHandlerResolver { get; private set; }

			/// <summary>
			/// Gets or sets the <see cref="ICorrelationIdHelper"/>.
			/// </summary>
			protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

			/// <summary>
			/// Gets or sets the <see cref="IAuthenticationTokenHelper{TAuthenticationToken}"/>.
			/// </summary>
			protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

			/// <summary>
			/// Passes the provided <paramref name="command"/> to <see cref="CommandHandlerResolver"/> via <see cref="ICommandPublisher{TAuthenticationToken}.Publish{TCommand}(TCommand)"/>
			/// then calls <see cref="ActorRefImplicitSenderExtensions.Tell"/>.
			/// </summary>
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