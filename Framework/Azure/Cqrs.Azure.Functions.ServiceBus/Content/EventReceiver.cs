using Azure.Messaging.ServiceBus;
using Cqrs.Azure.Functions.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

public class EventReceiver
{
	private readonly ILogger<EventReceiver> _logger;

	private readonly AzureFunctionCommandBusReceiver<Guid> _commandReceiver;

	public EventReceiver(ILogger<EventReceiver> log, AzureFunctionCommandBusReceiver<Guid> commandReceiver)
	{
		_logger = log;
		_commandReceiver = commandReceiver;
	}

	/// <summary>
	/// Receives a <see cref="ServiceBusReceivedMessage"/> from the private event bus.
	/// </summary>
	[Microsoft.Azure.Functions.Worker.Function("AzureFunctionCommandBusReceiver.Private")]
	public async virtual Task ReceiveCommandPrivate([Microsoft.Azure.Functions.Worker.ServiceBusTrigger("Cqrs.CommandBus.Private", nameof(EventReceiver), Connection = "Cqrs.Azure.CommandBus.ConnectionString")] ServiceBusReceivedMessage message)
	{
		await _commandReceiver.ReceiveCommand(null, message);
	}
	/// <summary>
	/// Receives a <see cref="ServiceBusReceivedMessage"/> from the public event bus.
	/// </summary>
	[Microsoft.Azure.Functions.Worker.Function("AzureFunctionCommandBusReceiver.Public")]
	public async virtual Task ReceiveCommandPublic([Microsoft.Azure.Functions.Worker.ServiceBusTrigger("Cqrs.CommandBus.Public", nameof(EventReceiver), Connection = "Cqrs.Azure.CommandBus.ConnectionString")] ServiceBusReceivedMessage message)
	{
		await _commandReceiver.ReceiveCommand(null, message);
	}

}