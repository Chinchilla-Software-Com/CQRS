using System;
using System.Collections.Generic;
using Cqrs.Events;
using Cqrs.Messages;

namespace Cqrs.Akka.Tests.Unit.Events
{
	public class HelloWorldRepliedTo : IEvent<Guid>
	{
		#region Implementation of IMessage

		public Guid CorrolationId { get; set; }
		public Guid CorrelationId { get { return CorrolationId; } set { CorrolationId = value; } }
		public FrameworkType Framework { get; set; }

		/// <summary>
		/// The originating framework this message was sent from.
		/// </summary>
		public string OriginatingFramework { get; set; }

		/// <summary>
		/// The frameworks this <see cref="IMessage"/> has been delivered to/sent via already.
		/// </summary>
		public IEnumerable<string> Frameworks { get; set; }

		#endregion

		#region Implementation of IMessageWithAuthenticationToken<Guid>

		public Guid AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IEvent<Guid>

		public Guid Id { get; set; }
		public int Version { get; set; }
		public DateTimeOffset TimeStamp { get; set; }

		#endregion
	}
}