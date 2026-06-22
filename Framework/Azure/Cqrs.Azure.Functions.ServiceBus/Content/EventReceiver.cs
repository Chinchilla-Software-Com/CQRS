using Azure.Messaging.ServiceBus;
using Cqrs.Azure.Functions.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

[ServiceBusAccount("ConnectionStrings:Cqrs.Azure.EventBus.ConnectionString")]
public class EventReceiver
{
	private readonly ILogger<EventReceiver> _logger;

	private readonly AzureEventBusReceiver<Guid> _eventReceiver;

	public EventReceiver(ILogger<EventReceiver> log, AzureEventBusReceiver<Guid> eventReceiver)
	{
		_logger = log;
		_eventReceiver = eventReceiver;
	}

	/// <summary>
	/// Receives a <see cref="ServiceBusReceivedMessage"/> from the private event bus.
	/// </summary>
	[FunctionName("AzureFunctionEventBusReceiver.Private")]
	public async virtual Task ReceiveEventPrivate([ServiceBusTrigger("Cqrs.EventBus.Private", nameof(EventReceiver), AutoCompleteMessages = false)] ServiceBusReceivedMessage message, ServiceBusClient client, ServiceBusMessageActions messageActions)
	{
		await _eventReceiver.ReceiveEventAsync(client, messageActions, message);
	}
	/// <summary>
	/// Receives a <see cref="ServiceBusReceivedMessage"/> from the public event bus.
	/// </summary>
	[FunctionName("AzureFunctionEventBusReceiver.Public")]
	public async virtual Task ReceiveEventPublic([ServiceBusTrigger("Cqrs.EventBus.Public", nameof(EventReceiver), AutoCompleteMessages = false)] ServiceBusReceivedMessage message, ServiceBusClient client, ServiceBusMessageActions messageActions)
	{
		await _eventReceiver.ReceiveEventAsync(client, messageActions, message);
	}

}