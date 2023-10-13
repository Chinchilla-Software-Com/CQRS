using Azure.Messaging.ServiceBus;
using Cqrs.Azure.Functions.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

public class IsolatedCommandReceiver
{
	private readonly ILogger<CommandReceiver> _logger;

	private readonly AzureCommandBusReceiver<Guid> _commandReceiver;

	public IsolatedCommandReceiver(ILogger<CommandReceiver> log, AzureCommandBusReceiver<Guid> commandReceiver)
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
		await _commandReceiver.ReceiveCommandAsync(null, message);
	}
	/// <summary>
	/// Receives a <see cref="ServiceBusReceivedMessage"/> from the public command bus.
	/// </summary>
	[Microsoft.Azure.Functions.Worker.Function("AzureFunctionCommandBusReceiver.Public")]
	public async virtual Task ReceiveCommandPublic([Microsoft.Azure.Functions.Worker.ServiceBusTrigger("Cqrs.CommandBus.Public", nameof(CommandReceiver), Connection = "Cqrs.Azure.CommandBus.ConnectionString")] ServiceBusReceivedMessage message)
	{
		await _commandReceiver.ReceiveCommandAsync(null, message);
	}

}