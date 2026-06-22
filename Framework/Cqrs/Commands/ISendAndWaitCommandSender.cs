#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Cqrs.Events;

namespace Cqrs.Commands
{
	/// <summary>
	/// Sends an <see cref="ICommand{TAuthenticationToken}"/>
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	[Obsolete("Use IPublishAndWaitCommandPublisher instead.")]
	public interface ISendAndWaitCommandSender<TAuthenticationToken> : ICommandPublisher<TAuthenticationToken>
	{
		/// <summary>
		/// Sends the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/>
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to send.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		TEvent SendAndWait<TCommand, TEvent>(TCommand command, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>;

		/// <summary>
		/// Sends the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/> or exits if the specified timeout is expired.
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to send.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		TEvent SendAndWait<TCommand, TEvent>(TCommand command, int millisecondsTimeout, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>;

		/// <summary>
		/// Sends the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/> or exits if the specified timeout is expired.
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to send.</param>
		/// <param name="timeout">A <see cref="T:System.TimeSpan"/> that represents the number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		TEvent SendAndWait<TCommand, TEvent>(TCommand command, TimeSpan timeout, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>;

		/// <summary>
		/// Sends the provided <paramref name="command"></paramref> and waits until the specified condition is satisfied an event of <typeparamref name="TEvent"/>
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to send.</param>
		/// <param name="condition">A delegate to be executed over and over until it returns the <typeparamref name="TEvent"/> that is desired, return null to keep trying.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		TEvent SendAndWait<TCommand, TEvent>(TCommand command, Func<IEnumerable<IEvent<TAuthenticationToken>>, TEvent> condition, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>;

		/// <summary>
		/// Sends the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/> or exits if the specified timeout is expired.
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to send.</param>
		/// <param name="condition">A delegate to be executed over and over until it returns the <typeparamref name="TEvent"/> that is desired, return null to keep trying.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		TEvent SendAndWait<TCommand, TEvent>(TCommand command, Func<IEnumerable<IEvent<TAuthenticationToken>>, TEvent> condition, int millisecondsTimeout, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>;

		/// <summary>
		/// Sends the provided <paramref name="command"></paramref> and waits for an event of <typeparamref name="TEvent"/> or exits if the specified timeout is expired.
		/// </summary>
		/// <param name="command">The <typeparamref name="TCommand"/> to send.</param>
		/// <param name="condition">A delegate to be executed over and over until it returns the <typeparamref name="TEvent"/> that is desired, return null to keep trying.</param>
		/// <param name="timeout">A <see cref="T:System.TimeSpan"/> that represents the number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="eventReceiver">If provided, is the <see cref="IEventReceiver{TAuthenticationToken}" /> that the event is expected to be returned on.</param>
		TEvent SendAndWait<TCommand, TEvent>(TCommand command, Func<IEnumerable<IEvent<TAuthenticationToken>>, TEvent> condition, TimeSpan timeout, IEventReceiver<TAuthenticationToken> eventReceiver = null)
			where TCommand : ICommand<TAuthenticationToken>;
	}
}