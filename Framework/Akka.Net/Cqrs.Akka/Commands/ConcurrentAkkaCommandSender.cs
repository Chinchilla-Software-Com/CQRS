#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Cqrs.Commands;
using Cqrs.Events;

namespace Cqrs.Akka.Commands
{
	/// <summary>
	/// A <see cref="IAkkaCommandPublisher{TAuthenticationToken}"/> that ensure concurrency regardless of what it passes the command onto as it is a <see cref="ReceiveActor"/>
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	/// <typeparam name="TTarget">The <see cref="Type"/> of the object that is targeted that needs concurrency.</typeparam>
	public class ConcurrentAkkaCommandPublisher<TAuthenticationToken, TTarget>
		: ReceiveActor
		, IConcurrentAkkaCommandPublisher<TAuthenticationToken, TTarget>
	{
		/// <summary>
		/// Gets the <see cref="IActorRef"/>.
		/// </summary>
		protected IActorRef ActorReference { get; private set; }

		/// <summary>
		/// Gets the <see cref="ICommandReceiver{TAuthenticationToken}"/>.
		/// </summary>
		protected ICommandReceiver<TAuthenticationToken> CommandReceiver { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="ConcurrentAkkaCommandPublisher{TAuthenticationToken,TTarget}"/>.
		/// </summary>
		public ConcurrentAkkaCommandPublisher(IActorRef actorReference, ICommandReceiver<TAuthenticationToken> commandReceiver)
		{
			ActorReference = actorReference;
			CommandReceiver = commandReceiver;
		}

		#region Implementation of ICommandPublisher<TAuthenticationToken>

		/// <summary>
		/// Publishes the provided <paramref name="command"/> on the <see cref="CommandReceiver"/>.
		/// </summary>
		/// <remarks>
		/// This is for when a command originated outside Akka and now needs to be pushed into Akka.
		/// </remarks>
		public void Publish<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			// This will trigger the Akka cycle back publishing... It looks weird, but trust it
			// This is for when a command originated outside Akka and now needs to be pushed into Akka
			CommandReceiver.ReceiveCommand(command);
		}

		/// <summary>
		/// Publishes the provided <paramref name="commands"/> on the <see cref="CommandReceiver"/>.
		/// </summary>
		/// <remarks>
		/// This is for when a command originated outside Akka and now needs to be pushed into Akka.
		/// </remarks>
		public void Publish<TCommand>(IEnumerable<TCommand> commands)
			where TCommand : ICommand<TAuthenticationToken>
		{
			// This will trigger the Akka cycle back publishing... It looks weird, but trust it
			// This is for when a command originated outside Akka and now needs to be pushed into Akka
			foreach (TCommand command in commands)
				CommandReceiver.ReceiveCommand(command);
		}

		/// <summary>
		/// Publishes the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/>
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to publish.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		public virtual TEvent PublishAndWait<TCommand, TEvent>(TCommand command, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>
		{
			return PublishAndWait<TCommand, TEvent>(command, -1, eventReceiver);
		}

		/// <summary>
		/// Publishes the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/> or exits if the specified timeout is expired.
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to publish.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		public TEvent PublishAndWait<TCommand, TEvent>(TCommand command, int millisecondsTimeout, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>
		{
			return PublishAndWait(command, events => (TEvent)events.SingleOrDefault(@event => @event is TEvent), millisecondsTimeout, eventReceiver);
		}

		/// <summary>
		/// Publishes the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/> or exits if the specified timeout is expired.
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to publish.</param>
		/// <param name="timeout">A <see cref="T:System.TimeSpan"/> that represents the number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		public TEvent PublishAndWait<TCommand, TEvent>(TCommand command, TimeSpan timeout, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>
		{
			long num = (long)timeout.TotalMilliseconds;
			if (num < -1L || num > int.MaxValue)
				throw new ArgumentOutOfRangeException("timeout", timeout, "SpinWait_SpinUntil_TimeoutWrong");
			return PublishAndWait<TCommand, TEvent>(command, (int)timeout.TotalMilliseconds, eventReceiver);
		}

		/// <summary>
		/// Publishes the provided <paramref name="command"></paramref> and waits until the specified condition is satisfied an event of <typeparamref name="TEvent"/>
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to publish.</param>
		/// <param name="condition">A delegate to be executed over and over until it returns the <typeparamref name="TEvent"/> that is desired, return null to keep trying.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		public TEvent PublishAndWait<TCommand, TEvent>(TCommand command, Func<IEnumerable<IEvent<TAuthenticationToken>>, TEvent> condition, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>
		{
			return PublishAndWait(command, condition, -1, eventReceiver);
		}

		/// <summary>
		/// Publishes the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/> or exits if the specified timeout is expired.
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to publish.</param>
		/// <param name="condition">A delegate to be executed over and over until it returns the <typeparamref name="TEvent"/> that is desired, return null to keep trying.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		public TEvent PublishAndWait<TCommand, TEvent>(TCommand command, Func<IEnumerable<IEvent<TAuthenticationToken>>, TEvent> condition, int millisecondsTimeout,
			IEventReceiver<TAuthenticationToken> eventReceiver = null) where TCommand : ICommand<TAuthenticationToken>
		{
			throw new NotImplementedException("This is a proxy so this wouldn't happen here.");
		}

		/// <summary>
		/// Publishes the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/> or exits if the specified timeout is expired.
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to publish.</param>
		/// <param name="condition">A delegate to be executed over and over until it returns the <typeparamref name="TEvent"/> that is desired, return null to keep trying.</param>
		/// <param name="timeout">A <see cref="T:System.TimeSpan"/> that represents the number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		public TEvent PublishAndWait<TCommand, TEvent>(TCommand command, Func<IEnumerable<IEvent<TAuthenticationToken>>, TEvent> condition, TimeSpan timeout, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>
		{
			long num = (long)timeout.TotalMilliseconds;
			if (num < -1L || num > int.MaxValue)
				throw new ArgumentOutOfRangeException("timeout", timeout, "SpinWait_SpinUntil_TimeoutWrong");
			return PublishAndWait(command, condition, (int)timeout.TotalMilliseconds, eventReceiver);
		}

		#endregion
	}
}