using System;
using Cqrs.Events;
using Cqrs.Repositories.Authentication;

namespace Cqrs.Tests.Substitutes
{
	public class TestAggregateCreated : IEvent<ISingleSignOnToken>
	{
		public Guid Id { get; set; }

		public int Version { get; set; }

		public DateTimeOffset TimeStamp { get; set; }

		#region Implementation of IMessageWithAuthenticationToken<ISingleSignOnToken>

		public ISingleSignOnToken AuthenticationToken { get; set; }

		#endregion
	}
}