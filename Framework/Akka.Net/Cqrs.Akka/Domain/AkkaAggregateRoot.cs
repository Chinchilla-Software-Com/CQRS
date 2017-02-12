#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Akka.Actor;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Commands;
using Cqrs.Domain;
using Cqrs.Domain.Exceptions;
using Cqrs.Events;
using Cqrs.Infrastructure;

namespace Cqrs.Akka.Domain
{
	public abstract class AkkaAggregateRoot<TAuthenticationToken>
		: ReceiveActor // PersistentActor 
		, IAggregateRoot<TAuthenticationToken>
	{
		protected IUnitOfWork<TAuthenticationToken> UnitOfWork { get; set; }

		protected IAkkaRepository<TAuthenticationToken> Repository { get; set; }

		protected ILogger Logger { get; set; }

		protected ICorrelationIdHelper CorrelationIdHelper { get; set; }

		protected IAuthenticationTokenHelper<TAuthenticationToken> AuthenticationTokenHelper { get; set; }

		private ICollection<IEvent<TAuthenticationToken>> Changes { get; set; }

		public Guid Id { get; protected set; }

		public int Version { get; protected set; }

		protected AkkaAggregateRoot(IUnitOfWork<TAuthenticationToken> unitOfWork, ILogger logger, IAkkaRepository<TAuthenticationToken> repository, ICorrelationIdHelper correlationIdHelper, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper)
		{
			UnitOfWork = unitOfWork;
			Logger = logger;
			Repository = repository;
			CorrelationIdHelper = correlationIdHelper;
			AuthenticationTokenHelper = authenticationTokenHelper;
			Changes = new ReadOnlyCollection<IEvent<TAuthenticationToken>>(new List<IEvent<TAuthenticationToken>>());
		}

		#region Overrides of ActorBase

		/// <summary>
		/// User overridable callback.
		///                 <p/>
		///                 Is called when an Actor is started.
		///                 Actors are automatically started asynchronously when created.
		///                 Empty default implementation.
		/// </summary>
		protected override void PreStart()
		{
			base.PreStart();
			Repository.LoadAggregateHistory(this, throwExceptionOnNoEvents: false);
		}

		#endregion

		protected virtual void Execute<TCommand>(Action<TCommand> action, TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			UnitOfWork.Add(this);
			try
			{
				AuthenticationTokenHelper.SetAuthenticationToken(command.AuthenticationToken);
				CorrelationIdHelper.SetCorrelationId(command.CorrelationId);
				action(command);

				UnitOfWork.Commit();

				Sender.Tell(true, Self);
			}
			catch(Exception exception)
			{
				Logger.LogError("Executing an Akka.net request failed.", exception: exception, metaData: new Dictionary<string, object> { { "Type", GetType() }, { "Command", command } });
				Sender.Tell(false, Self);
				throw;
			}
		}

		public IEnumerable<IEvent<TAuthenticationToken>> GetUncommittedChanges()
		{
			return Changes;
		}

		public virtual void MarkChangesAsCommitted()
		{
			Version = Version + Changes.Count;
			Changes = new ReadOnlyCollection<IEvent<TAuthenticationToken>>(new List<IEvent<TAuthenticationToken>>());
		}

		public virtual void LoadFromHistory(IEnumerable<IEvent<TAuthenticationToken>> history)
		{
			Type aggregateType = GetType();
			foreach (IEvent<TAuthenticationToken> @event in history.OrderBy(e =>e.Version))
			{
				if (@event.Version != Version + 1)
					throw new EventsOutOfOrderException(@event.Id, aggregateType, Version + 1, @event.Version);
				ApplyChange(@event, true);
			}
		}

		protected virtual void ApplyChange(IEvent<TAuthenticationToken> @event)
		{
			ApplyChange(@event, false);
		}

		private void ApplyChange(IEvent<TAuthenticationToken> @event, bool isEventReplay)
		{
			this.AsDynamic().Apply(@event);
			if (!isEventReplay)
			{
				Changes = new ReadOnlyCollection<IEvent<TAuthenticationToken>>(new []{@event}.Concat(Changes).ToList());
			}
			else
			{
				Id = @event.Id;
				Version++;
			}
		}
	}
}