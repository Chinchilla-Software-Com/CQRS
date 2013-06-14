using CQRSCode.ReadModel;
using CQRSCode.WriteModel;
using Cqrs.Bus;
using Cqrs.Cache;
using Cqrs.Commands;
using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Cqrs.Events;
using StructureMap;
using StructureMap.Graph;

namespace CQRSWeb {
	public static class IoC {
		public static IContainer Initialize() {
			ObjectFactory.Initialize(x =>
						{
							x.For<InProcessBus>().Singleton().Use<InProcessBus>();
							x.For<IAggregateFactory>().Singleton().Use<AggregateFactory>();
							x.For<ICommandSender>().Use(y => y.GetInstance<InProcessBus>());
							x.For<IEventPublisher>().Use(y => y.GetInstance<InProcessBus>());
							x.For<IHandlerRegistrar>().Use(y => y.GetInstance<InProcessBus>());
							x.For<IUnitOfWork>().HybridHttpOrThreadLocalScoped().Use<UnitOfWork>();
							x.For<IEventStore>().Singleton().Use<InMemoryEventStore>();
							x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use(y =>
																					 new CacheRepository(
																						 new Repository(y.GetInstance<IAggregateFactory>(), y.GetInstance<IEventStore>(), y.GetInstance<IEventPublisher>()),
																						 y.GetInstance<IEventStore>()));

							// Scan the assembly the ReadModelFacade class is in and then configure using the pattern
							// IClass == Class
							x.Scan(s =>
							{
								s.TheCallingAssembly();
								s.AssemblyContainingType<ReadModelFacade>();
								s.Convention<FirstInterfaceConvention>();
							});
						});
			return ObjectFactory.Container;
		}
	}
}