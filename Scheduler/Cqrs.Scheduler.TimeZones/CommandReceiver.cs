using Azure.Messaging.ServiceBus;
using Cqrs.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

public class CommandReceiver
{
	private readonly ILogger<CommandReceiver> _logger;

	private readonly AzureCommandBusReceiver<Guid> _commandReceiver;

	public CommandReceiver(ILogger<CommandReceiver> log, AzureCommandBusReceiver<Guid> commandReceiver)
	{
		_logger = log;
		_commandReceiver = commandReceiver;
	}

	/// <summary>
	/// Receives a <see cref="ServiceBusReceivedMessage"/> from the command bus.
	/// </summary>
	[Microsoft.Azure.Functions.Worker.Function("Timzezones.Isolated.Private")]
	public async virtual Task ReceiveCommandPrivate([Microsoft.Azure.Functions.Worker.ServiceBusTrigger("Cqrs.Scheduler.Commands-Local.Private", "Cqrs.Timezone-Publisher", Connection = "Cqrs.Azure.CommandBus.ConnectionString")] ServiceBusReceivedMessage message)
	{
		await _commandReceiver.ReceiveCommandAsync((ServiceBusReceiver)null, message);
	}
	/// <summary>
	/// Receives a <see cref="ServiceBusReceivedMessage"/> from the command bus.
	/// </summary>
	[Microsoft.Azure.Functions.Worker.Function("Timzezones.Isolated.Public")]
	public async virtual Task ReceiveCommandPublic([Microsoft.Azure.Functions.Worker.ServiceBusTrigger("Cqrs.Scheduler.Commands-Local.Public", "Cqrs.Timezone-Publisher", Connection = "Cqrs.Azure.CommandBus.ConnectionString")] ServiceBusReceivedMessage message)
	{
		await _commandReceiver.ReceiveCommandAsync((ServiceBusReceiver)null, message);
	}
}