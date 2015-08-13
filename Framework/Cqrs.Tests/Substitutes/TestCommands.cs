using System;
using Cqrs.Commands;
using Cqrs.Authentication;

namespace Cqrs.Tests.Substitutes
{
	public class TestAggregateDoSomething : ICommand<ISingleSignOnToken>
	{
		public Guid Id { get; set; }

		public int ExpectedVersion { get; set; }

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

	public class TestAggregateDoSomethingHandler : ICommandHandler<ISingleSignOnToken, TestAggregateDoSomething> 
	{
		public void Handle(TestAggregateDoSomething message)
		{
			TimesRun++;
		}

		public int TimesRun { get; set; }
	}
	public class TestAggregateDoSomethingElseHandler : ICommandHandler<ISingleSignOnToken, TestAggregateDoSomething>
	{
		public void Handle(TestAggregateDoSomething message)
		{

		}
	}
}