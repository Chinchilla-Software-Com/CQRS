#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Azure.ServiceBus.Tests.Unit;
using Cqrs.Configuration;
using Cqrs.Domain;
using Cqrs.Domain.Factories;
using Cqrs.Events;
using Cqrs.Messages;
using Cqrs.MongoDB.Events;
using Cqrs.MongoDB.Tests.Integration.Configuration;
using Cqrs.Ninject.Configuration;
using Cqrs.Snapshots;
using MongoDB.Driver;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;

namespace Cqrs.MongoDB.Tests.Integration
{
	/// <summary>
	/// A series of tests on the <see cref="MongoDbSnapshotStore"/> class
	/// </summary>
	[TestClass]
	public class MongoDbSnapshotStoreTests
	{
		/// <summary>
		/// Tests the <see cref="ISnapshotStore.Save"/> method
		/// Passing a valid test <see cref="IEvent{TAuthenticationToken}"/>
		/// Expecting the test <see cref="IEvent{TAuthenticationToken}"/> is able to be read.
		/// </summary>
		[TestMethod]
		public void Should_load_events()
		{
			// Arrange
			TestMongoDbSnapshotStoreConnectionStringFactory.DatabaseName = string.Format("Test-{0}", new Random().Next(0, 9999));
			NinjectDependencyResolver.ModulesToLoad.Add(new CqrsModule<int, DefaultAuthenticationTokenHelper>());
			NinjectDependencyResolver.ModulesToLoad.Add(new InProcessEventBusModule<int>());
			NinjectDependencyResolver.ModulesToLoad.Add(new TestMongoDbModule<int>());
			NinjectDependencyResolver.Start();
			var unitOfWork = new UnitOfWork<int>(DependencyResolver.Current.Resolve<ISnapshotAggregateRepository<int>>());
			var aggregate = DependencyResolver.Current.Resolve<IAggregateFactory>().Create<TestAggregate>(Guid.NewGuid());
			unitOfWork.Add(aggregate);
			try
			{
				int count = 0;
				do
				{
					aggregate.GenerateRandomNumber();
					if (count % 10 == 0)
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
			finally
			{
				// Clean-up
				TestMongoDataStoreConnectionStringFactory.DatabaseName = TestMongoDbSnapshotStoreConnectionStringFactory.DatabaseName;
				var factory = new TestMongoDbDataStoreFactory(DependencyResolver.Current.Resolve<ILogger>(), new TestMongoDataStoreConnectionStringFactory());
				IMongoCollection<TestEvent> collection = factory.GetTestEventCollection();
				collection.Database.Client.DropDatabase(TestMongoDataStoreConnectionStringFactory.DatabaseName);
			}
		}

		/// <summary />
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

			/// <summary />
			public Guid Rsn
			{
				get { return Id; }
				set { Id = value; }
			}

			/// <summary />
			public int CurrentRandomNumber { get; set; }

			/// <summary />
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
				return new TestAggregateSnapshot { CurrentRandomNumber = CurrentRandomNumber };
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

		/// <summary />
		public class TestAggregateSnapshot
			: Snapshot
		{
			/// <summary />
			public int CurrentRandomNumber { get; set; }
		}

		/// <summary />
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
			/// The AuthenticationToken of the entity that triggered the event to be raised.
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

			/// <summary />
			public int RandomNumber { get; set; }

			/// <summary />
			public RandomNumberEvent(Guid rsn)
			{
				Id = Guid.NewGuid();
				Rsn = rsn;
				RandomNumber = new Random().Next(0, 99999);
			}
		}
	}
}