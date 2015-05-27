using Cqrs.Authentication;
using Cqrs.Commands;
using Cqrs.Configuration;
using cdmdotnet.Logging;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureCommandBusPublisher<TAuthenticationToken> : AzureCommandBus<TAuthenticationToken>, ICommandSender<TAuthenticationToken>
	{
		public AzureCommandBusPublisher(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrolationIdHelper corrolationIdHelper)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, corrolationIdHelper)
		{
		}

		#region Implementation of ICommandSender<TAuthenticationToken>

		public void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			command.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();
			command.CorrolationId = CorrolationIdHelper.GetCorrolationId();

			ServiceBusClient.Send(new BrokeredMessage(MessageSerialiser.SerialiseCommand(command)));
		}

		#endregion
	}
}
