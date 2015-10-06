#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Messages;

namespace Cqrs.Events
{
	public class DuplicateCreateCommandEvent<TAuthenticationToken> : IEvent<TAuthenticationToken>
	{
		#region Implementation of IMessage

		public Guid CorrolationId { get; set; }

		public Guid CorrelationId { get; set; }

		public FrameworkType Framework { get; set; }

		#endregion

		#region Implementation of IMessageWithAuthenticationToken<TAuthenticationToken>

		public TAuthenticationToken AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IEvent<TAuthenticationToken>

		public Guid Id { get; set; }

		public int Version { get; set; }

		public DateTimeOffset TimeStamp { get; set; }

		#endregion

		public Type AggregateType { get; set; }

		public Guid AggregateRsn { get; set; }
	}
}