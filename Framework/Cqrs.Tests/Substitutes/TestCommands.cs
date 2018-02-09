using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Cqrs.Commands;
using Cqrs.Authentication;
using Cqrs.Messages;

namespace Cqrs.Tests.Substitutes
{
	public class TestAggregateDoSomething
		: ICommand<ISingleSignOnToken>
		, ITelemeteredMessage
	{
		public Guid Id { get; set; }

		public int ExpectedVersion { get; set; }

		#region Implementation of IMessageWithAuthenticationToken<ISingleSignOnToken>

		public ISingleSignOnToken AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IMessage

		[DataMember]
		public Guid CorrelationId { get; set; }

		/// <summary>
		/// The originating framework this message was sent from.
		/// </summary>
		[DataMember]
		public string OriginatingFramework { get; set; }

		/// <summary>
		/// The frameworks this <see cref="IMessage"/> has been delivered to/sent via already.
		/// </summary>
		[DataMember]
		public IEnumerable<string> Frameworks { get; set; }

		#endregion

		public TestAggregateDoSomething()
		{
			TelemetryName = "Command/TestAggregateDoSomething";
		}

		#region Implementation of ITelemeteredMessage

		/// <summary>
		/// Gets or sets the Name of this message.
		/// </summary>
		public string TelemetryName { get; set; }

		#endregion
	}

	public class TestAggregateDoSomething2 : TestAggregateDoSomething
	{
	}

	public class TestAggregateDoSomething3 : TestAggregateDoSomething
	{
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