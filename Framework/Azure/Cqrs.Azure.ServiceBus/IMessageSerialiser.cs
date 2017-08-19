#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Commands;
using Cqrs.Events;

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// Serialises <see cref="IEvent{TAuthenticationToken}">events</see> and <see cref="ICommand{TAuthenticationToken}">commands</see>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public interface IMessageSerialiser<TAuthenticationToken>
	{
		/// <summary>
		/// Serialise the provided <paramref name="event"/>.
		/// </summary>
		/// <typeparam name="TEvent">The <see cref="Type"/> of the <see cref="IEvent{TAuthenticationToken}"/> being serialised.</typeparam>
		/// <param name="event">The <see cref="IEvent{TAuthenticationToken}"/> being serialised.</param>
		/// <returns>A <see cref="string"/> representation of the provided <paramref name="event"/>.</returns>
		string SerialiseEvent<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>;

		/// <summary>
		/// Serialise the provided <paramref name="command"/>.
		/// </summary>
		/// <typeparam name="TCommand">The <see cref="Type"/> of the <see cref="ICommand{TAuthenticationToken}"/> being serialised.</typeparam>
		/// <param name="command">The <see cref="ICommand{TAuthenticationToken}"/> being serialised.</param>
		/// <returns>A <see cref="string"/> representation of the provided <paramref name="command"/>.</returns>
		string SerialiseCommand<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>;

		/// <summary>
		/// Deserialise the provided <paramref name="event"/> from its <see cref="string"/> representation.
		/// </summary>
		/// <param name="event">A <see cref="string"/> representation of an <see cref="IEvent{TAuthenticationToken}"/> to deserialise.</param>
		IEvent<TAuthenticationToken> DeserialiseEvent(string @event);

		/// <summary>
		/// Deserialise the provided <paramref name="command"/> from its <see cref="string"/> representation.
		/// </summary>
		/// <param name="command">A <see cref="string"/> representation of an <see cref="ICommand{TAuthenticationToken}"/> to deserialise.</param>
		ICommand<TAuthenticationToken> DeserialiseCommand(string command);
	}
}