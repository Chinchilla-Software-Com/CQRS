using CQRSCode.ReadModel;
using CQRSCode.WriteModel;
using Cqrs.Bus;
using Cqrs.Cache;
using Cqrs.Commands;
using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Cqrs.Events;
using Cqrs.Repositories.Authentication;
using StructureMap;
using StructureMap.Graph;

namespace CQRSWeb {
	public static class IoC {
		public static IContainer Initialize() {
			ObjectFactory.Initialize(x =>
						{
							x.For<InProcessBus<ISingleSignOnToken>>().Singleton().Use<InProcessBus<ISingleSignOnToken>>();
							x.For<IAggregateFactory>().Singleton().Use<AggregateFactory>();
							x.For<ICommandSender<ISingleSignOnToken>>().Use(y => y.GetInstance<InProcessBus<ISingleSignOnToken>>());
							x.For<IEventPublisher<ISingleSignOnToken>>().Use(y => y.GetInstance<InProcessBus<ISingleSignOnToken>>());
							x.For<IHandlerRegistrar>().Use(y => y.GetInstance<InProcessBus<ISingleSignOnToken>>());
							x.For<IUnitOfWork<ISingleSignOnToken>>().HybridHttpOrThreadLocalScoped().Use<UnitOfWork<ISingleSignOnToken>>();
							x.For<IEventStore<ISingleSignOnToken>>().Singleton().Use<InMemoryEventStore>();
							x.For<IRepository<ISingleSignOnToken>>().HybridHttpOrThreadLocalScoped().Use(y =>
																					 new CacheRepository<ISingleSignOnToken>(
																						 new Repository<ISingleSignOnToken>(y.GetInstance<IAggregateFactory>(), y.GetInstance<IEventStore<ISingleSignOnToken>>(), y.GetInstance<IEventPublisher<ISingleSignOnToken>>()),
																						 y.GetInstance<IEventStore<ISingleSignOnToken>>()));

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