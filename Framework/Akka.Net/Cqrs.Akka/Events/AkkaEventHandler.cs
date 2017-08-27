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
using Cqrs.Authentication;
using Cqrs.Events;

namespace Cqrs.Akka.Events
{
	/// <summary>
	/// Executes event handler methods.
	/// </summary>
	/// <typeparam name="TAuthenticationToken"></typeparam>
	public abstract class AkkaEventHandler<TAuthenticationToken>
		: ReceiveActor // PersistentActor 
	{
		/// <summary>
		/// Gets or sets the <see cref="ILogger"/>.
		/// </summary>
		protected ILogger Logger { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="ICorrelationIdHelper"/>.
		/// </summary>
		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		/// <summary>
		/// Gets or sets the <see cref="IAuthenticationTokenHelper{TAuthenticationToken}"/>.
		/// </summary>
		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="AkkaEventHandler{TAuthenticationToken}"/>.
		/// </summary>
		protected AkkaEventHandler(ILogger logger, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
		{
			Logger = logger;
			CorrelationIdHelper = correlationIdHelper;
			AuthenticationTokenHelper = authenticationTokenHelper;
		}

		/// <summary>
		/// Execute the provided <paramref name="handler"/> passing it the <paramref name="event"/>,
		/// then calls <see cref="ActorRefImplicitSenderExtensions.Tell"/>.
		/// </summary>
		protected virtual void Execute<TEvent>(Action<TEvent> handler, TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			try
			{
				AuthenticationTokenHelper.SetAuthenticationToken(@event.AuthenticationToken);
				CorrelationIdHelper.SetCorrelationId(@event.CorrelationId);
				handler(@event);

				Sender.Tell(true, Self);
			}
			catch(Exception exception)
			{
				Logger.LogError("Executing an Akka.net request failed.", exception: exception, metaData: new Dictionary<string, object> { { "Type", GetType() }, { "Event", @event} });
				Sender.Tell(false, Self);
				throw;
			}
		}
	}
}