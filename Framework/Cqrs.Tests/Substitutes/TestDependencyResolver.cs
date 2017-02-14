using System;
using System.Collections.Generic;
using cdmdotnet.Logging;
using cdmdotnet.Logging.Configuration;
using Cqrs.Bus;
using Cqrs.Configuration;

namespace Cqrs.Tests.Substitutes
{
	public class TestDependencyResolver : IDependencyResolver
	{
		protected TestEventStore TestEventStore { get; private set; }

		public bool UseTestEventStoreGuid { get; set; }

		public Guid? NewAggregateGuid { get; set; }

		public readonly List<dynamic> Handlers = new List<dynamic>();

		public TestDependencyResolver(TestEventStore testEventStore)
		{
			TestEventStore = testEventStore;
		}

		public T Resolve<T>()
		{
			return (T)Resolve(typeof(T));
		}

		public object Resolve(Type type)
		{
			if (type == typeof(IHandlerRegistrar) || type == typeof(IEventHandlerRegistrar) || type == typeof(ICommandHandlerRegistrar))
				return new TestHandleRegistrar();
			if (type == typeof(ILogger))
				return new ConsoleLogger(new LoggerSettingsConfigurationSection(), new NullCorrelationIdHelper());
			if (type == typeof (IConfigurationManager))
				return new ConfigurationManager();
			if (type == typeof(TestAggregate))
				return new TestAggregate(TestEventStore == null || !UseTestEventStoreGuid ? NewAggregateGuid ?? Guid.NewGuid() : TestEventStore.EmptyGuid);
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