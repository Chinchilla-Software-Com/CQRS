using Azure.Messaging.ServiceBus;
using Cqrs.Azure.Functions.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

public class IsolatedEventReceiver
{
	private readonly ILogger<EventReceiver> _logger;

	private readonly AzureEventBusReceiver<Guid> _eventReceiver;

	public IsolatedEventReceiver(ILogger<EventReceiver> log, AzureEventBusReceiver<Guid> eventReceiver)
	{
		_logger = log;
		_eventReceiver = eventReceiver;
	}

	/// <summary>
	/// Receives a <see cref="ServiceBusReceivedMessage"/> from the private event bus.
	/// </summary>
	[Microsoft.Azure.Functions.Worker.Function("AzureFunctionEventBusReceiver.Private")]
	public async virtual Task ReceiveEventPrivate([Microsoft.Azure.Functions.Worker.ServiceBusTrigger("Cqrs.EventBus.Private", nameof(EventReceiver), Connection = "Cqrs.Azure.EventBus.ConnectionString")] ServiceBusReceivedMessage message)
	{
		await _eventReceiver.ReceiveEventAsync(null, message);
	}
	/// <summary>
	/// Receives a <see cref="ServiceBusReceivedMessage"/> from the public event bus.
	/// </summary>
	[Microsoft.Azure.Functions.Worker.Function("AzureFunctionEventBusReceiver.Public")]
	public async virtual Task ReceiveEventPublic([Microsoft.Azure.Functions.Worker.ServiceBusTrigger("Cqrs.EventBus.Public", nameof(EventReceiver), Connection = "Cqrs.Azure.EventBus.ConnectionString")] ServiceBusReceivedMessage message)
	{
		await _eventReceiver.ReceiveEventAsync(null, message);
	}

}