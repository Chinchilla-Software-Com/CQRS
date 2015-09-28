#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Commands;
using Cqrs.Events;

namespace Cqrs.Azure.ServiceBus
{
	public interface IMessageSerialiser<TAuthenticationToken>
	{
		string SerialiseEvent<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>;

		string SerialiseCommand<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>;

		IEvent<TAuthenticationToken> DeserialiseEvent(string @event);

		ICommand<TAuthenticationToken> DeserialiseCommand(string @event);
	}
}