#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Authentication;
using Cqrs.Configuration;
using cdmdotnet.Logging;

namespace Cqrs.Azure.ServiceBus
{
	public abstract class AzureCommandBus<TAuthenticationToken> : AzureEventHub<TAuthenticationToken>
	{
		protected override string EventHubConnectionStringNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.CommandBus.ConnectionStringName"; }
		}

		protected override string EventHubStorageConnectionStringNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.CommandBus.StorageConnectionStringName"; }
		}

		protected override string PrivateEventHubNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.CommandBus.PrivateEvent.EventHubName"; }
		}

		protected override string PublicEventHubNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.CommandBus.PublicEvent.EventHubName"; }
		}

		protected override string PrivateEventHubConsumerGroupNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.CommandBus.PrivateEvent.EventHubName.ConsumerGroupName"; }
		}

		protected override string PublicEventHubConsumerGroupNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.CommandBus.PublicEvent.EventHubName.ConsumerGroupName"; }
		}

		protected override string DefaultPrivateEventHubName
		{
			get { return "Cqrs.EventHub.CommandBus.Private"; }
		}

		protected override string DefaultPublicEventHubName
		{
			get { return "Cqrs.EventHub.CommandBus"; }
		}

		protected IAzureBusHelper<TAuthenticationToken> AzureBusHelper { get; private set; }


		protected AzureCommandBus(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, bool isAPublisher)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, isAPublisher)
		{
			AzureBusHelper = azureBusHelper;
		}
	}
}
