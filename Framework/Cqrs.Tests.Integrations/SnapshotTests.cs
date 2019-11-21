using System;
using System.Collections.Generic;
using Chinchilla.Logging;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Cqrs.Events;
using Cqrs.Messages;
using Cqrs.Ninject.Configuration;
using Cqrs.Snapshots;
using NUnit.Framework;

namespace Cqrs.Tests.Integrations
{
	[TestFixture]
	public class SnapshotTests
	{
		[Test]
		public void Should_load_events()
		{
			// Arrange
			NinjectDependencyResolver.ModulesToLoad.Add(new CqrsModule<int, DefaultAuthenticationTokenHelper>());
			NinjectDependencyResolver.ModulesToLoad.Add(new SimplifiedSqlModule<int>());
			NinjectDependencyResolver.ModulesToLoad.Add(new InProcessEventBusModule<int>());
			NinjectDependencyResolver.Start();
			var unitOfWork = new UnitOfWork<int>(DependencyResolver.Current.Resolve<ISnapshotAggregateRepository<int>>());
			var aggregate = DependencyResolver.Current.Resolve<IAggregateFactory>().Create<TestAggregate>(Guid.NewGuid());
			unitOfWork.Add(aggregate);
			int count = 0;
			do
			{
				aggregate.GenerateRandomNumber();
				if (count%10 == 0)
				{
					unitOfWork.Commit();
					unitOfWork.Add(aggregate);
				}
			} while (count++ <= 20);
			unitOfWork.Commit();

			// Act
			var aggregate2 = unitOfWork.Get<TestAggregate>(aggregate.Rsn);

			// Assert
			Assert.AreEqual(22, aggregate2.Version);
			Assert.AreEqual(aggregate.CurrentRandomNumber, aggregate2.CurrentRandomNumber);
		}

		public class TestAggregate
			: SnapshotAggregateRoot<int, TestAggregateSnapshot>
		{
			/// <summary>
			/// Gets or sets the <see cref="IDependencyResolver"/> used.
			/// </summary>
			protected IDependencyResolver DependencyResolver { get; private set; }

			/// <summary>
			/// Gets or sets the <see cref="ILogger"/> used.
			/// </summary>
			protected ILogger Logger { get; private set; }

			/// <summary>
			/// Instantiates a new instance of <see cref="AggregateFactory"/>.
			/// </summary>
			public TestAggregate(IDependencyResolver dependencyResolver, ILogger logger, Guid rsn)
			{
				DependencyResolver = dependencyResolver;
				Logger = logger;
				Rsn = rsn;
			}

			public Guid Rsn
			{
				get { return Id; }
				set { Id = value; }
			}

			public int CurrentRandomNumber { get; set; }

			public void GenerateRandomNumber()
			{
				ApplyChange(new RandomNumberEvent(Rsn));
			}

			private void Apply(RandomNumberEvent @event)
			{
				CurrentRandomNumber = @event.RandomNumber;
			}

			#region Overrides of SnapshotAggregateRoot<int,TestAggregateSnapshot>

			/// <summary>
			/// Create a <see cref="TestAggregateSnapshot"/> of the current state of this instance.
			/// </summary>
			protected override TestAggregateSnapshot CreateSnapshot()
			{
				return new TestAggregateSnapshot {CurrentRandomNumber = CurrentRandomNumber};
			}

			/// <summary>
			/// Rehydrate this instance from the provided <paramref name="snapshot"/>.
			/// </summary>
			/// <param name="snapshot">The <see cref="TestAggregateSnapshot"/> to rehydrate this instance from.</param>
			protected override void RestoreFromSnapshot(TestAggregateSnapshot snapshot)
			{
				CurrentRandomNumber = snapshot.CurrentRandomNumber;
			}

			#endregion
		}

		public class TestAggregateSnapshot
			: Snapshot
		{
			public int CurrentRandomNumber { get; set; }
		}

		public class RandomNumberEvent
			: IEventWithIdentity<int>
		{
			#region Implementation of IMessage

			/// <summary>
			/// An identifier used to group together several <see cref="IMessage"/>. Any <see cref="IMessage"/> with the same <see cref="CorrelationId"/> were triggered by the same initiating request.
			/// </summary>
			public Guid CorrelationId { get; set; }

			/// <summary>
			/// The originating framework this message was sent from.
			/// </summary>
			public string OriginatingFramework { get; set; }

			/// <summary>
			/// The frameworks this <see cref="IMessage"/> has been delivered to/sent via already.
			/// </summary>
			public IEnumerable<string> Frameworks { get; set; }

			#endregion

			#region Implementation of IMessageWithAuthenticationToken<int>

			/// <summary>
			/// The <typeparamref name="TAuthenticationToken"/> of the entity that triggered the event to be raised.
			/// </summary>
			public int AuthenticationToken { get; set; }

			#endregion

			#region Implementation of IEvent<int>

			/// <summary>
			/// The ID of the <see cref="IEvent{TAuthenticationToken}"/>
			/// </summary>
			public Guid Id { get; set; }

			/// <summary>
			/// The version of the <see cref="IEvent{TAuthenticationToken}"/>
			/// </summary>
			public int Version { get; set; }

			/// <summary>
			/// The date and time the event was raised or published.
			/// </summary>
			public DateTimeOffset TimeStamp { get; set; }

			#endregion

			#region Implementation of IEventWithIdentity<int>

			/// <summary>
			/// The identity of the <see cref="IAggregateRoot{TAuthenticationToken}">aggregate</see> being targeted.
			/// </summary>
			public Guid Rsn { get; set; }

			#endregion

			public int RandomNumber { get; set; }

			public RandomNumberEvent(Guid rsn)
			{
				Id = Guid.NewGuid();
				Rsn = rsn;
				RandomNumber = new Random().Next(0, 99999);
			}
		}
	}
}