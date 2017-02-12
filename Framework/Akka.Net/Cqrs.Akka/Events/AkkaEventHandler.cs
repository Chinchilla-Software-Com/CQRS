#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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
	public abstract class AkkaEventHandler<TAuthenticationToken>
		: ReceiveActor // PersistentActor 
	{
		protected ILogger Logger { get; set; }

		protected ICorrelationIdHelper CorrelationIdHelper { get; private set; }

		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; private set; }

		protected AkkaEventHandler(ILogger logger, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
		{
			Logger = logger;
			CorrelationIdHelper = correlationIdHelper;
			AuthenticationTokenHelper = authenticationTokenHelper;
		}

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