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
using Cqrs.Bus;

namespace Cqrs.Azure.ServiceBus
{
	public abstract class AzureEventHubBus<TAuthenticationToken> : AzureEventHub<TAuthenticationToken>
	{
		protected override string EventHubConnectionStringNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.EventBus.ConnectionStringName"; }
		}

		protected override string EventHubStorageConnectionStringNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.EventBus.StorageConnectionStringName"; }
		}

		protected override string PrivateEventHubNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.EventBus.PrivateEvent.EventHubName"; }
		}

		protected override string PublicEventHubNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.EventBus.PublicEvent.EventHubName"; }
		}

		protected override string PrivateEventHubConsumerGroupNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.EventBus.PrivateEvent.EventHubName.ConsumerGroupName"; }
		}

		protected override string PublicEventHubConsumerGroupNameConfigurationKey
		{
			get { return "Cqrs.Azure.EventHub.EventBus.PublicEvent.EventHubName.ConsumerGroupName"; }
		}

		protected override string DefaultPrivateEventHubName
		{
			get { return "Cqrs.EventHub.EventBus.Private"; }
		}

		protected override string DefaultPublicEventHubName
		{
			get { return "Cqrs.EventHub.EventBus"; }
		}

		protected IAzureBusHelper<TAuthenticationToken> AzureBusHelper { get; private set; }

		protected AzureEventHubBus(IConfigurationManager configurationManager, IBusHelper busHelper, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, bool isAPublisher)
			: base(configurationManager, busHelper, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, isAPublisher)
		{
			AzureBusHelper = azureBusHelper;
		}
	}
}
