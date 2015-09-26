using System;
using System.Runtime.Serialization;
using Cqrs.Commands;
using Cqrs.Authentication;
using Cqrs.Messages;

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

		[DataMember]
		public FrameworkType Framework { get; set; }

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