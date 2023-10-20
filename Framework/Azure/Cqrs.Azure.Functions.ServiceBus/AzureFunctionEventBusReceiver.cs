#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Chinchilla.Logging;
using Cqrs.Authentication;
using Cqrs.Azure.ServiceBus;
using Cqrs.Bus;
using Cqrs.Events;
using Cqrs.Configuration;
using ServiceBusMessageActions = Microsoft.Azure.WebJobs.ServiceBus.ServiceBusMessageActions;

namespace Cqrs.Azure.Functions.ServiceBus
{
	/// <summary>
	/// A <see cref="IAsyncEventReceiver{TAuthenticationToken}"/> that receives network messages, resolves handlers and executes the handler.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	// The “,nq” suffix here just asks the expression evaluator to remove the quotes when displaying the final value (nq = no quotes).
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class AzureFunctionEventBusReceiver<TAuthenticationToken>
		: AzureEventBusReceiver<TAuthenticationToken>
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="AzureFunctionEventBusReceiver{TAuthenticationToken}"/>.
		/// </summary>
		public AzureFunctionEventBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper, IHashAlgorithmFactory hashAlgorithmFactory)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, hashAlgorithmFactory)
		{
		}

		/// <summary>
		/// Receives a <see cref="ServiceBusClient"/>, <see cref="ServiceBusMessageActions"/> along with the <see cref="ServiceBusReceivedMessage"/> from the event bus.
		/// </summary>
		public async virtual Task ReceiveEventAsync(ServiceBusClient client, ServiceBusMessageActions messageActions, ServiceBusReceivedMessage message)
		{
			FieldInfo[] fields = messageActions.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Default | BindingFlags.GetField);

			FieldInfo messageReceiverField = fields.SingleOrDefault(x => x.FieldType == typeof(ServiceBusReceiver));
			ServiceBusReceiver messageReceiver = (ServiceBusReceiver)messageReceiverField.GetValue(messageActions);

			if (messageReceiver != null)
			{
				await ReceiveEventAsync(messageReceiver, message);
				return;
			}

			FieldInfo processMessageEventArgsField = fields.SingleOrDefault(x => x.FieldType == typeof(ProcessMessageEventArgs));
			ProcessMessageEventArgs processMessageEventArgs = (ProcessMessageEventArgs)processMessageEventArgsField.GetValue(messageActions);

			await ReceiveEventAsync(processMessageEventArgs);
		}
	}
}