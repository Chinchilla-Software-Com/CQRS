using System;
using Cqrs.Authentication;
using Cqrs.Commands;
using Cqrs.Configuration;
using cdmdotnet.Logging;
using Microsoft.ServiceBus.Messaging;

namespace Cqrs.Azure.ServiceBus
{
	public class AzureCommandBusPublisher<TAuthenticationToken> : AzureCommandBus<TAuthenticationToken>, ICommandSender<TAuthenticationToken>
	{
		protected IDependencyResolver DependencyResolver { get; private set; }

		public AzureCommandBusPublisher(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IDependencyResolver dependencyResolver)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, true, false)
		{
			DependencyResolver = dependencyResolver;
		}

		#region Implementation of ICommandSender<TAuthenticationToken>

		public void Send<TCommand>(TCommand command)
			where TCommand : ICommand<TAuthenticationToken>
		{
			ICommandValidator<TAuthenticationToken, TCommand> commandValidator = null;
			try
			{
				commandValidator = DependencyResolver.Resolve<ICommandValidator<TAuthenticationToken, TCommand>>();
			}
			catch (Exception exception)
			{
				Logger.LogDebug("Locating an ICommandValidator failed.", string.Format("{0}\\Handle({1})", GetType().FullName, command.GetType().FullName), exception);
			}

			if (commandValidator != null && !commandValidator.IsCommandValid(command))
			{
				Logger.LogInfo("The provided command is not valid.", string.Format("{0}\\Handle({1})", GetType().FullName, command.GetType().FullName));
				return;
			}

			if (command.AuthenticationToken == null)
				command.AuthenticationToken = AuthenticationTokenHelper.GetAuthenticationToken();
			command.CorrelationId = CorrelationIdHelper.GetCorrelationId();

			ServiceBusPublisher.Send(new BrokeredMessage(MessageSerialiser.SerialiseCommand(command)));
			Logger.LogInfo(string.Format("A command was sent of type {0}.", command.GetType().FullName));
		}

		#endregion
	}
}
