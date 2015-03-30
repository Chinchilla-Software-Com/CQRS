using Cqrs.Commands;
using Cqrs.Configuration;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureCommandBusPublisher<TAuthenticationToken> : AzureCommandBus<TAuthenticationToken>, ICommandSender<TAuthenticationToken>
	{
		public AzureCommandBusPublisher(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser)
			: base(configurationManager, messageSerialiser)
		{
		}

		#region Implementation of ICommandSender<TAuthenticationToken>

		public void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			command.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();

			ServiceBusClient.Send(new BrokeredMessage(MessageSerialiser.SerialiseCommand(command)));
		}

		#endregion
	}
}
