using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using Chinchilla.Logging;

namespace Cqrs.Akka.Domain
{
	/// <summary>
	/// Proxy deadletters to <see cref="ILogger"/>.
	/// </summary>
	public class DeadletterToLoggerProxy : ReceiveActor
	{
		private ILogger Logger { get; set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="DeadletterToLoggerProxy"/>
		/// </summary>
		public DeadletterToLoggerProxy(ILogger logger)
		{
			Logger = logger;
			Receive<DeadLetter>(dl => HandleDeadletter(dl));
		}

		private void HandleDeadletter(DeadLetter dl)
		{
			string message = dl.Message.ToString();
			bool value;
			if (bool.TryParse(message, out value) && value)
				return;
			Logger.LogWarning("Akka delivery failed", dl.Recipient.Path.ToString(), new Exception(dl.Message.ToString()), metaData: new Dictionary<string, object>{{"Sender", dl.Sender}});
		}
	}
}