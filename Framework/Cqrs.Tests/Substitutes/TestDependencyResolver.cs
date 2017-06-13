using System;
using System.Collections.Generic;
using cdmdotnet.Logging;
using cdmdotnet.Logging.Configuration;
using Cqrs.Authentication;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;

namespace Cqrs.Tests.Substitutes
{
	public class TestDependencyResolver : IDependencyResolver
	{
		protected TestEventStore TestEventStore { get; private set; }

		protected ICommandPublisher<ISingleSignOnToken> TestSingleSignOnTokenCommandPublisher { get; private set; }

		public bool UseTestEventStoreGuid { get; set; }

		public Guid? NewAggregateGuid { get; set; }

		public readonly List<dynamic> Handlers = new List<dynamic>();

		public TestDependencyResolver(TestEventStore testEventStore, ICommandPublisher<ISingleSignOnToken> testSingleSignOnTokenCommandPublisher = null)
		{
			TestEventStore = testEventStore;
			TestSingleSignOnTokenCommandPublisher = testSingleSignOnTokenCommandPublisher;
		}

		public T Resolve<T>()
		{
			return (T)Resolve(typeof(T));
		}

		public object Resolve(Type type)
		{
			if (type == typeof(ILogger))
				return new ConsoleLogger(new LoggerSettings(), new NullCorrelationIdHelper());
			if (type == typeof(IDependencyResolver))
				return this;
			if (type == typeof(ICommandPublisher<ISingleSignOnToken>))
				return TestSingleSignOnTokenCommandPublisher;
			if (type == typeof(IHandlerRegistrar) || type == typeof(IEventHandlerRegistrar) || type == typeof(ICommandHandlerRegistrar))
				return new TestHandleRegistrar();
			if (type == typeof(ILogger))
				return new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper());
			if (type == typeof (IConfigurationManager))
				return new ConfigurationManager();
			if (type == typeof(TestAggregate))
				return new TestAggregate(TestEventStore == null || !UseTestEventStoreGuid ? NewAggregateGuid ?? Guid.NewGuid() : TestEventStore.EmptyGuid);
			if (type == typeof(TestSaga))
				return new TestSaga(this, TestEventStore == null || !UseTestEventStoreGuid ? NewAggregateGuid ?? Guid.NewGuid() : TestEventStore.EmptyGuid);
			if (type == typeof(TestSnapshotAggregate))
				return new TestSnapshotAggregate(TestEventStore == null || !UseTestEventStoreGuid ? NewAggregateGuid ?? Guid.NewGuid() : TestEventStore.EmptyGuid);
			if (type == typeof(TestAggregateDidSomethingHandler))
			{
				var handler = new TestAggregateDidSomethingHandler();
				Handlers.Add(handler);
				return handler;
			}
			else
			{
				var handler = new TestAggregateDoSomethingHandler();
				Handlers.Add(handler);
				return handler;
			}
		}
	}
}