using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Cqrs.Events;
using Cqrs.Authentication;
using Cqrs.Messages;

namespace Cqrs.Tests.Substitutes
{
	public class TestAggregateDidSomething
		: IEvent<ISingleSignOnToken>
		, ITelemeteredMessage
	{
		public Guid Id { get; set; }

		public int Version { get; set; }

		public DateTimeOffset TimeStamp { get; set; }

		#region Implementation of IMessageWithAuthenticationToken<ISingleSignOnToken>

		public ISingleSignOnToken AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IMessage

		[DataMember]
		public Guid CorrelationId { get; set; }

		[DataMember]
		[Obsolete("Use Frameworks, It's far more flexible and OriginatingFramework")]
		public FrameworkType Framework { get; set; }

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

		[Obsolete("Use CorrelationId")]
		[DataMember]
		public Guid CorrolationId
		{
			get { return CorrelationId; }
			set { CorrelationId = value; }
		}

		#endregion

		public TestAggregateDidSomething()
		{
			TelemetryName = "Event/TestAggregateDidSomething";
		}

		#region Implementation of ITelemeteredMessage

		/// <summary>
		/// Gets or sets the Name of this message.
		/// </summary>
		public string TelemetryName { get; set; }

		#endregion
	}

	public class TestAggregateDidSomethingElse : IEvent<ISingleSignOnToken>
	{
		public Guid Id { get; set; }

		public int Version { get; set; }

		public DateTimeOffset TimeStamp { get; set; }

		#region Implementation of IMessageWithAuthenticationToken<ISingleSignOnToken>

		public ISingleSignOnToken AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IMessage

		[DataMember]
		public Guid CorrelationId { get; set; }

		[DataMember]
		[Obsolete("Use Frameworks, It's far more flexible and OriginatingFramework")]
		public FrameworkType Framework { get; set; }

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

		[Obsolete("Use CorrelationId")]
		[DataMember]
		public Guid CorrolationId
		{
			get { return CorrelationId; }
			set { CorrelationId = value; }
		}

		#endregion
	}

	public class TestAggregateDidSomethingElse2 : TestAggregateDidSomethingElse
	{
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
