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
using Chinchilla.Logging;
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

		private ICollection<ICommand<TAuthenticationToken>> Commands { get; set; }

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
		protected
#if NET40
			ICommandPublisher
#else
			IAsyncCommandPublisher
#endif
			<TAuthenticationToken> CommandPublisher { get; private set; }

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
			Commands = new ReadOnlyCollection<ICommand<TAuthenticationToken>>(new List<ICommand<TAuthenticationToken>>());
			Initialise();
		}

		/// <summary>
		/// Initialise any properties
		/// </summary>
		protected virtual void Initialise()
		{
		}

		/// <summary>
		/// A constructor for the <see cref="Cqrs.Domain.Factories.IAggregateFactory"/>
		/// </summary>
		protected Saga(IDependencyResolver dependencyResolver, ILogger logger)
			: this()
		{
			DependencyResolver = dependencyResolver;
			Logger = logger;
			CommandPublisher = DependencyResolver.Resolve
			<
#if NET40
				ICommandPublisher
#else
				IAsyncCommandPublisher
#endif
				<TAuthenticationToken>
			>();
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
					throw new EventsOutOfOrderException(@event.Rsn, sagaType, Version + 1, @event.Version);
				ApplyChange(@event, true);
			}
		}

		/// <summary>
		/// Get all pending commands that haven't yet been published yet.
		/// </summary>
		public virtual IEnumerable<ICommand<TAuthenticationToken>> GetUnpublishedCommands()
		{
			return Commands;
		}

		/// <summary>
		/// Queue the provided <paramref name="command"/> for publishing.
		/// </summary>
		protected virtual void QueueCommand(ICommand<TAuthenticationToken> command)
		{
			Lock.EnterWriteLock();
			try
			{
				Commands = new ReadOnlyCollection<ICommand<TAuthenticationToken>>(Commands.Concat(new []{command}).ToList());
			}
			finally
			{
				Lock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Mark all published commands as published and flush the internal collection of commands.
		/// </summary>
		public virtual void MarkCommandsAsPublished()
		{
			Lock.EnterWriteLock();
			try
			{
				Commands = new ReadOnlyCollection<ICommand<TAuthenticationToken>>(new List<ICommand<TAuthenticationToken>>());
			}
			finally
			{
				Lock.ExitWriteLock();
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

		/// <summary>
		/// Sets the <see cref="IEvent{TAuthenticationToken}.Id"/> from <see cref="ISagaEvent{TAuthenticationToken}.Event"/> back onto <paramref name="sagaEvent"/>.
		/// </summary>
		protected virtual void SetId(ISagaEvent<TAuthenticationToken> sagaEvent)
		{
			sagaEvent.Rsn = sagaEvent.Event.GetIdentity();
		}

		private void ApplyChange(ISagaEvent<TAuthenticationToken> @event, bool isEventReplay)
		{
			ApplyChanges(new[] { @event }, isEventReplay);
		}

		/// <summary>
		/// Call the "Apply" method with a signature matching each <see cref="ISagaEvent{TAuthenticationToken}"/> in the provided <paramref name="events"/> without using event replay to this instance.
		/// </summary>
		/// <remarks>
		/// This means a method named "Apply", with return type void and one parameter must exist to be applied.
		/// If no method exists, nothing is applied
		/// The parameter type must match exactly the <see cref="Type"/> of the <see cref="IEvent{TAuthenticationToken}"/> in the provided <paramref name="events"/>.
		/// </remarks>
		protected virtual void ApplyChanges(IEnumerable<ISagaEvent<TAuthenticationToken>> events)
		{
			ApplyChanges(events, false);
		}

		/// <summary>
		/// Calls the "SetId" method dynamically if the method exists on the first <see cref="IEvent{TAuthenticationToken}"/> in the provided <paramref name="events"/>,
		/// then calls <see cref="ApplyChanges(System.Collections.Generic.IEnumerable{Cqrs.Events.ISagaEvent{TAuthenticationToken}})"/>
		/// </summary>
		protected virtual void ApplyChanges(IEnumerable<IEvent<TAuthenticationToken>> events)
		{
			IList<IEvent<TAuthenticationToken>> list = events.ToList();
			IList<ISagaEvent<TAuthenticationToken>> sagaEvents = new List<ISagaEvent<TAuthenticationToken>>();
			for (int i = 0; i < list.Count; i++)
			{
				var sagaEvent = new SagaEvent<TAuthenticationToken>(list[i]);
				// Set ID
				if (i == 0)
					this.AsDynamic().SetId(sagaEvent);
				sagaEvents.Add(sagaEvent);
			}
			ApplyChanges(sagaEvents);
		}

		private void ApplyChanges(IEnumerable<ISagaEvent<TAuthenticationToken>> events, bool isEventReplay)
		{
			Lock.EnterWriteLock();
			IList<ISagaEvent<TAuthenticationToken>> changes = new List<ISagaEvent<TAuthenticationToken>>();
			try
			{
				try
				{
					dynamic dynamicThis = this.AsDynamic();
					foreach (ISagaEvent<TAuthenticationToken> @event in events)
					{
						dynamicThis.Apply(@event.Event);
						if (!isEventReplay)
						{
							changes.Add(@event);
						}
						else
						{
							Id = @event.Rsn;
							Version++;
						}
					}
				}
				finally
				{
					Changes = new ReadOnlyCollection<ISagaEvent<TAuthenticationToken>>(Changes.Concat(changes).ToList());
				}
			}
			finally
			{
				Lock.ExitWriteLock();
			}
		}
	}
}