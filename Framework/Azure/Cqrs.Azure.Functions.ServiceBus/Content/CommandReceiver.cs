using Azure.Messaging.ServiceBus;
using Cqrs.Azure.Functions.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

[ServiceBusAccount("ConnectionStrings:Cqrs.Azure.CommandBus.ConnectionString")]
public class CommandReceiver
{
	private readonly ILogger<CommandReceiver> _logger;

	private readonly AzureFunctionCommandBusReceiver<Guid> _commandReceiver;

	public CommandReceiver(ILogger<CommandReceiver> log, AzureFunctionCommandBusReceiver<Guid> commandReceiver)
	{
		_logger = log;
		_commandReceiver = commandReceiver;
	}

	/// <summary>
	/// Receives a <see cref="ServiceBusReceivedMessage"/> from the private command bus.
	/// </summary>
	[FunctionName("AzureFunctionCommandBusReceiver-Private")]
	public async virtual Task ReceiveCommandPrivate([ServiceBusTrigger("Cqrs.CommandBus.Private", nameof(CommandReceiver), AutoCompleteMessages = false)] ServiceBusReceivedMessage message, ServiceBusClient client, ServiceBusMessageActions messageActions)
	{
		await _commandReceiver.ReceiveCommandAsync(client, messageActions, message);
	}
	/// <summary>
	/// Receives a <see cref="ServiceBusReceivedMessage"/> from the public command bus.
	/// </summary>
	[FunctionName("AzureFunctionCommandBusReceiver-Public")]
	public async virtual Task ReceiveCommandPublic([ServiceBusTrigger("Cqrs.CommandBus.Public", nameof(CommandReceiver), AutoCompleteMessages = false)] ServiceBusReceivedMessage message, ServiceBusClient client, ServiceBusMessageActions messageActions)
	{
		await _commandReceiver.ReceiveCommandAsync(client, messageActions, message);
	}
}