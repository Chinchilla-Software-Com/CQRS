#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Chinchilla.Logging;
using Cqrs.Authentication;
using Cqrs.Azure.ServiceBus;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;

namespace Cqrs.Azure.Functions.ServiceBus
{
	/// <summary>
	/// A <see cref="ICommandReceiver{TAuthenticationToken}"/> that receives network messages, resolves handlers and executes the handler.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	// The “,nq” suffix here just asks the expression evaluator to remove the quotes when displaying the final value (nq = no quotes).
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class AzureFunctionCommandBusReceiver<TAuthenticationToken>
		: AzureCommandBusReceiver<TAuthenticationToken>
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="AzureCommandBusReceiver{TAuthenticationToken}"/>.
		/// </summary>
		public AzureFunctionCommandBusReceiver(IConfigurationManager configurationManager, IMessageSerialiser<TAuthenticationToken> messageSerialiser, IAuthenticationTokenHelper<TAuthenticationToken> authenticationTokenHelper, ICorrelationIdHelper correlationIdHelper, ILogger logger, IAzureBusHelper<TAuthenticationToken> azureBusHelper, IBusHelper busHelper, IHashAlgorithmFactory hashAlgorithmFactory)
			: base(configurationManager, messageSerialiser, authenticationTokenHelper, correlationIdHelper, logger, azureBusHelper, busHelper, hashAlgorithmFactory)
		{
		}

		/*
		/// <summary>
		/// Receives a <see cref="ICommand{TAuthenticationToken}"/> from the command bus.
		/// </summary>
		public override async Task<bool?> ReceiveCommandAsync(ICommand<TAuthenticationToken> command)
		{
			return await AzureBusHelper.DefaultReceiveCommandAsync(command, Routes, "Azure-Function-ServiceBus");
		}
		*/

		protected override async Task InstantiatePublishingAsync()
		{
			await base.InstantiatePublishingAsync();
		}
	}
}