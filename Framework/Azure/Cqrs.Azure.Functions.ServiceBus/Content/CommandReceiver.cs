using Azure.Messaging.ServiceBus;
using Cqrs.Azure.Functions.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

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
	[Microsoft.Azure.Functions.Worker.Function("AzureFunctionCommandBusReceiver.Private")]
	public async virtual Task ReceiveCommandPrivate([Microsoft.Azure.Functions.Worker.ServiceBusTrigger("Cqrs.CommandBus.Private", nameof(CommandReceiver), Connection = "Cqrs.Azure.CommandBus.ConnectionString")] ServiceBusReceivedMessage message)
	{
		await _commandReceiver.ReceiveCommand(null, message);
	}
	/// <summary>
	/// Receives a <see cref="ServiceBusReceivedMessage"/> from the public command bus.
	/// </summary>
	[Microsoft.Azure.Functions.Worker.Function("AzureFunctionCommandBusReceiver.Public")]
	public async virtual Task ReceiveCommandPublic([Microsoft.Azure.Functions.Worker.ServiceBusTrigger("Cqrs.CommandBus.Public", nameof(CommandReceiver), Connection = "Cqrs.Azure.CommandBus.ConnectionString")] ServiceBusReceivedMessage message)
	{
		await _commandReceiver.ReceiveCommand(null, message);
	}

}