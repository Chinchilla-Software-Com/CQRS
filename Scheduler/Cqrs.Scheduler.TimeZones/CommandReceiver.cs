using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Chinchilla.StateManagement;
using Cqrs.Azure.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public class CommandReceiver
{
	private readonly ILogger<CommandReceiver> _logger;

	private readonly AzureCommandBusReceiver<Guid> _commandReceiver;

	private readonly IContextItemCollection _backChannel;

	public CommandReceiver(ILogger<CommandReceiver> log, AzureCommandBusReceiver<Guid> commandReceiver, IContextItemCollectionFactory contextItemCollectionFactory)
	{
		_logger = log;
		_commandReceiver = commandReceiver;
		_backChannel = contextItemCollectionFactory.GetCurrentContext();
	}

	/// <summary>
	/// Receives a <see cref="ServiceBusReceivedMessage"/> from the command bus.
	/// </summary>
	[Microsoft.Azure.Functions.Worker.Function("Timzezones.Isolated.Private")]
	public async virtual Task ReceiveCommandPrivate([Microsoft.Azure.Functions.Worker.ServiceBusTrigger("Cqrs.Scheduler.Commands-Local.Private", "Cqrs.Timezone-Publisher", Connection = "Cqrs.Azure.CommandBus.ConnectionString")] ServiceBusReceivedMessage message, FunctionContext executionContext)
	{
		_backChannel.SetData("Cqrs.Azure.FunctionName", executionContext?.Features?.Get<FunctionDefinition>()?.Name);
		await _commandReceiver.ReceiveCommandAsync((ServiceBusReceiver)null, message);
	}
	/// <summary>
	/// Receives a <see cref="ServiceBusReceivedMessage"/> from the command bus.
	/// </summary>
	[Microsoft.Azure.Functions.Worker.Function("Timzezones.Isolated.Public")]
	public async virtual Task ReceiveCommandPublic([Microsoft.Azure.Functions.Worker.ServiceBusTrigger("Cqrs.Scheduler.Commands-Local.Public", "Cqrs.Timezone-Publisher", Connection = "Cqrs.Azure.CommandBus.ConnectionString")] ServiceBusReceivedMessage message, FunctionContext executionContext)
	{
		_backChannel.SetData("Cqrs.Azure.FunctionName", executionContext?.Features?.Get<FunctionDefinition>()?.Name);
		await _commandReceiver.ReceiveCommandAsync((ServiceBusReceiver)null, message);
	}
}