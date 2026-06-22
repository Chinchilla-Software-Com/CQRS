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
using Cqrs.Commands;
using Cqrs.Configuration;
using ServiceBusMessageActions = Microsoft.Azure.WebJobs.ServiceBus.ServiceBusMessageActions;

namespace Cqrs.Azure.Functions.ServiceBus
{
	/// <summary>
	/// A <see cref="IAsyncCommandReceiver{TAuthenticationToken}"/> that receives network messages, resolves handlers and executes the handler.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	// The “,nq” suffix here just asks the expression evaluator to remove the quotes when displaying the final value (nq = no quotes).
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class AzureFunctionCommandBusReceiver<TAuthenticationToken>
		: AzureCommandBusReceiver<TAuthenticationToken>
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="AzureFunctionCommandBusReceiver{TAuthenticationToken}"/>.
		/// </summary>
		public AzureFunctionCommandBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper, IHashAlgorithmFactory hashAlgorithmFactory)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, hashAlgorithmFactory)
		{
		}

		/// <summary>
		/// Receives a <see cref="ServiceBusClient"/>, <see cref="ServiceBusMessageActions"/> along with the <see cref="ServiceBusReceivedMessage"/> from the command bus.
		/// </summary>
		public async virtual Task ReceiveCommandAsync(ServiceBusClient client, ServiceBusMessageActions messageActions, ServiceBusReceivedMessage message)
		{
			FieldInfo[] fields = messageActions.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Default | BindingFlags.GetField);

			FieldInfo messageReceiverField = fields.SingleOrDefault(x => x.FieldType == typeof(ServiceBusReceiver));
			ServiceBusReceiver messageReceiver = (ServiceBusReceiver)messageReceiverField.GetValue(messageActions);

			if (messageReceiver != null)
			{
				await ReceiveCommandAsync(messageReceiver, message);
				return;
			}

			FieldInfo processMessageEventArgsField = fields.SingleOrDefault(x => x.FieldType == typeof(ProcessMessageEventArgs));
			ProcessMessageEventArgs processMessageEventArgs = (ProcessMessageEventArgs)processMessageEventArgsField.GetValue(messageActions);

			await ReceiveCommandAsync(processMessageEventArgs);
		}
	}
}