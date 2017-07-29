#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using cdmdotnet.Logging;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Domain.Exceptions;
using Cqrs.Events;
using Cqrs.Infrastructure;

namespace Cqrs.Domain
{
	/// <summary>
	/// An independent component that reacts to domain <see cref="IEvent{TAuthenticationToken}"/> in a cross-<see cref="IAggregateRoot{TAuthenticationToken}"/>, eventually consistent manner. Time can also be a trigger. A <see cref="Saga{TAuthenticationToken}"/> can sometimes be purely reactive, and sometimes represent workflows.
	/// 
	/// From an implementation perspective, a <see cref="Saga{TAuthenticationToken}"/> is a state machine that is driven forward by incoming <see cref="IEvent{TAuthenticationToken}"/> (which may come from many <see cref="AggregateRoot{TAuthenticationToken}"/> or other <see cref="Saga{TAuthenticationToken}"/>). Some states will have side effects, such as sending <see cref="ICommand{TAuthenticationToken}"/>, talking to external web services, or sending emails.
	/// </summary>
	/// <remarks>
	/// Isn't a <see cref="Saga{TAuthenticationToken}"/> just leaked domain logic?
	/// No.
	/// A <see cref="Saga{TAuthenticationToken}"/> can doing things that no individual <see cref="AggregateRoot{TAuthenticationToken}"/> can sensibly do. Thus, it's not a logic leak since the logic didn't belong in an <see cref="AggregateRoot{TAuthenticationToken}"/> anyway. Furthermore, we're not breaking encapsulation in any way, since <see cref="Saga{TAuthenticationToken}"/> operate with <see cref="ICommand{TAuthenticationToken}"/> and <see cref="IEvent{TAuthenticationToken}"/>, which are part of the public API.
	/// 
	/// How can I make my <see cref="Saga{TAuthenticationToken}"/> react to an <see cref="IEvent{TAuthenticationToken}"/> that did not happen?
	/// The <see cref="Saga{TAuthenticationToken}"/>, besides reacting to domain <see cref="IEvent{TAuthenticationToken}"/>, can be "woken up" by recurrent internal alarms. Implementing such alarms is easy. See cron in Unix, or triggered WebJobs in Azure for examples.
	/// 
	/// How does the <see cref="Saga{TAuthenticationToken}"/> interact with the write side?
	/// By sending an <see cref="ICommand{TAuthenticationToken}"/> to it.
	/// </remarks>
	public abstract class Saga<TAuthenticationToken> : ISaga<TAuthenticationToken>
	{
		private ReaderWriterLockSlim Lock { get; set; }

		private ICollection<ISagaEvent<TAuthenticationToken>> Changes { get; set; }

		/// <summary>
		/// The identifier of this <see cref="ISaga{TAuthenticationToken}"/>.
		/// </summary>
		[DataMember]
		public Guid Rsn
		{
			get { return Id; }
			private set { Id = value; }
		}

		/// <summary>
		/// The identifier of this <see cref="ISaga{TAuthenticationToken}"/>.
		/// </summary>
		[DataMember]
		public Guid Id { get; protected set; }

		/// <summary>
		/// The current version of this <see cref="ISaga{TAuthenticationToken}"/>.
		/// </summary>
		public int Version { get; protected set; }

		/// <summary>
		/// Gets or set the <see cref="ICommandPublisher{TAuthenticationToken}"/>.
		/// </summary>
		protected ICommandPublisher<TAuthenticationToken> CommandPublisher { get; private set; }

		/// <summary>
		/// Gets or set the <see cref="IDependencyResolver"/>.
		/// </summary>
		protected IDependencyResolver DependencyResolver { get; private set; }

		/// <summary>
		/// Gets or set the <see cref="ILogger"/>.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// A constructor for the <see cref="Cqrs.Domain.Factories.IAggregateFactory"/>
		/// </summary>
		protected Saga()
		{
			Lock = new ReaderWriterLockSlim();
			Changes = new ReadOnlyCollection<ISagaEvent<TAuthenticationToken>>(new List<ISagaEvent<TAuthenticationToken>>());
		}

		/// <summary>
		/// A constructor for the <see cref="Cqrs.Domain.Factories.IAggregateFactory"/>
		/// </summary>
		protected Saga(IDependencyResolver dependencyResolver, ILogger logger)
			: this()
		{
			DependencyResolver = dependencyResolver;
			Logger = logger;
			CommandPublisher = DependencyResolver.Resolve<ICommandPublisher<TAuthenticationToken>>();
		}

		/// <summary>
		/// A constructor for the <see cref="Cqrs.Domain.Factories.IAggregateFactory"/>
		/// </summary>
		protected Saga(IDependencyResolver dependencyResolver, ILogger logger, Guid rsn)
			: this(dependencyResolver, logger)
		{
			Rsn = rsn;
		}

		/// <summary>
		/// Get all applied changes that haven't yet been committed.
		/// </summary>
		public virtual IEnumerable<ISagaEvent<TAuthenticationToken>> GetUncommittedChanges()
		{
			return Changes;
		}

		/// <summary>
		/// Mark all applied changes as committed, increment <see cref="Version"/> and flush the <see cref="Changes">internal collection of changes</see>.
		/// </summary>
		public virtual void MarkChangesAsCommitted()
		{
			Lock.EnterWriteLock();
			try
			{
				Version = Version + Changes.Count;
				Changes = new ReadOnlyCollection<ISagaEvent<TAuthenticationToken>>(new List<ISagaEvent<TAuthenticationToken>>());
			}
			finally
			{
				Lock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Apply all the <see cref="IEvent{TAuthenticationToken}">events</see> in <paramref name="history"/>
		/// using event replay to this instance.
		/// </summary>
		public virtual void LoadFromHistory(IEnumerable<ISagaEvent<TAuthenticationToken>> history)
		{
			Type sagaType = GetType();
			foreach (ISagaEvent<TAuthenticationToken> @event in history.OrderBy(e => e.Version))
			{
				if (@event.Version != Version + 1)
					throw new EventsOutOfOrderException(@event.Id, sagaType, Version + 1, @event.Version);
				ApplyChange(@event, true);
			}
		}

		/// <summary>
		/// Call the "Apply" method with a signature matching the provided <paramref name="event"/> without using event replay to this instance.
		/// </summary>
		/// <remarks>
		/// This means a method named "Apply", with return type void and one parameter must exist to be applied.
		/// If no method exists, nothing is applied
		/// The parameter type must match exactly the <see cref="Type"/> of the provided <paramref name="event"/>.
		/// </remarks>
		protected virtual void ApplyChange(ISagaEvent<TAuthenticationToken> @event)
		{
			ApplyChange(@event, false);
		}

		/// <summary>
		/// Calls the "SetId" method dynamically if the method exists, then calls <see cref="ApplyChange(Cqrs.Events.ISagaEvent{TAuthenticationToken})"/>
		/// </summary>
		protected virtual void ApplyChange(IEvent<TAuthenticationToken> @event)
		{
			var sagaEvent = new SagaEvent<TAuthenticationToken>(@event);
			// Set ID
			this.AsDynamic().SetId(sagaEvent);
			ApplyChange(sagaEvent);
		}

		private void ApplyChange(ISagaEvent<TAuthenticationToken> @event, bool isEventReplay)
		{
			Lock.EnterWriteLock();
			try
			{
				this.AsDynamic().Apply(@event.Event);
				if (!isEventReplay)
				{
					Changes = new ReadOnlyCollection<ISagaEvent<TAuthenticationToken>>(Changes.Concat(new[] { @event }).ToList());
				}
				else
				{
					Id = @event.Id;
					Version++;
				}
			}
			finally
			{
				Lock.ExitWriteLock();
			}
		}
	}
}