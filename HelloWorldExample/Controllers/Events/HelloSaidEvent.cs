using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Cqrs.Events;

namespace HelloWorldExample.Controllers.Events
{
	[NotifyCallerEvent]
	[Serializable]
	[DataContract]
	public class HelloSaidEvent : IEvent<string>
	{
		#region Implementation of IMessage

		[DataMember]
		public Guid CorrelationId { get; set; }

		/// <summary>
		/// The originating framework this message was sent from.
		/// </summary>
		[DataMember]
		public string OriginatingFramework { get; set; }

		/// <summary>
		/// The frameworks this <see cref="T:Cqrs.Messages.IMessage"/> has been delivered to/sent via already.
		/// </summary>
		[DataMember]
		public IEnumerable<string> Frameworks { get; set; }

		#endregion

		#region Implementation of IMessageWithAuthenticationToken<string>

		[DataMember]
		public string AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IEvent<string>

		[DataMember]
		public Guid Id { get; set; }

		[DataMember]
		public int Version { get; set; }

		[DataMember]
		public DateTimeOffset TimeStamp { get; set; }

		#endregion
	}
}