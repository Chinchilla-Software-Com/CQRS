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

namespace Cqrs.Akka.Events
{
	public abstract class AkkaEventHandler
		: ReceiveActor // PersistentActor 
	{
		protected ILogger Logger { get; set; }

		protected AkkaEventHandler(ILogger logger)
		{
			Logger = logger;
		}

		protected virtual void Execute<TEvent>(Action<TEvent> handler, TEvent @event)
		{
			try
			{
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