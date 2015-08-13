using System;
using Cqrs.Events;
using Cqrs.Authentication;

namespace Cqrs.Tests.Substitutes
{
	public class TestAggregateDidSomething : IEvent<ISingleSignOnToken>
	{
		public Guid Id { get; set; }

		public int Version { get; set; }

		public DateTimeOffset TimeStamp { get; set; }

		#region Implementation of IMessageWithAuthenticationToken<ISingleSignOnToken>

		public ISingleSignOnToken AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IMessage

		public Guid CorrelationId { get; set; }

		[Obsolete("Use CorrelationId")]
		public Guid CorrolationId
		{
			get { return CorrelationId; }
			set { CorrelationId = value; }
		}

		#endregion
	}

	public class TestAggregateDidSomeethingElse : IEvent<ISingleSignOnToken>
	{
		public Guid Id { get; set; }

		public int Version { get; set; }

		public DateTimeOffset TimeStamp { get; set; }

		#region Implementation of IMessageWithAuthenticationToken<ISingleSignOnToken>

		public ISingleSignOnToken AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IMessage

		public Guid CorrelationId { get; set; }

		[Obsolete("Use CorrelationId")]
		public Guid CorrolationId
		{
			get { return CorrelationId; }
			set { CorrelationId = value; }
		}

		#endregion
	}

	public class TestAggregateDidSomethingHandler : IEventHandler<ISingleSignOnToken, TestAggregateDidSomething>
	{
		public void Handle(TestAggregateDidSomething message)
		{
			lock (message)
			{
				TimesRun++;
			}
		}

		public int TimesRun { get; private set; }
	}
}
