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

	private readonly AzureFunctionEventBusReceiver<Guid> _eventReceiver;

	public EventReceiver(ILogger<EventReceiver> log, AzureFunctionEventBusReceiver<Guid> eventReceiver)
	{
		_logger = log;
		_eventReceiver = eventReceiver;
	}

	/// <summary>
	/// Receives a <see cref="ServiceBusReceivedMessage"/> from the event bus.
	/// </summary>
	[FunctionName("SampleReports-Private")]
	public async virtual Task ReceiveEventPrivate([ServiceBusTrigger("%Cqrs:Azure:EventBus:PrivateEvent:Topic:Name%", "%Cqrs:Azure:EventBus:PrivateEvent:Topic:Subscription:Name%", AutoCompleteMessages = false)] ServiceBusReceivedMessage message, ServiceBusClient client, ServiceBusMessageActions messageActions)
	{
		await _eventReceiver.ReceiveEventAsync(client, messageActions, message);
	}
	/// <summary>
	/// Receives a <see cref="ServiceBusReceivedMessage"/> from the event bus.
	/// </summary>
	[FunctionName("SampleReports-Public")]
	public async virtual Task ReceiveEventPublic([ServiceBusTrigger("%Cqrs:Azure:EventBus:PublicEvent:Topic:Name%", "%Cqrs:Azure:EventBus:PublicEvent:Topic:Subscription:Name%", AutoCompleteMessages = false)] ServiceBusReceivedMessage message, ServiceBusClient client, ServiceBusMessageActions messageActions)
	{
		await _eventReceiver.ReceiveEventAsync(client, messageActions, message);
	}
}