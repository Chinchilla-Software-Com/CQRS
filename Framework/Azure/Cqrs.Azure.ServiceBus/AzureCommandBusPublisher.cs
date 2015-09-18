using Cqrs.Authentication;
using Cqrs.Commands;
using Cqrs.Configuration;
using cdmdotnet.Logging;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureCommandBusPublisher<TAuthenticationToken> : AzureCommandBus<TAuthenticationToken>, ICommandSender<TAuthenticationToken>
	{
		public AzureCommandBusPublisher(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, true, false)
		{
		}

		#region Implementation of ICommandSender<TAuthenticationToken>

		public void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			if (command.AuthenticationToken == null)
				command.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();
			command.CorrelationId = CorrelationIdHelper.GetCorrelationId();

			ServiceBusPublisher.Send(new BrokeredMessage(MessageSerialiser.SerialiseCommand(command)));
			Logger.LogInfo(string.Format("A command was sent of type {0}.", command.GetType().FullName));
		}

		#endregion
	}
}
