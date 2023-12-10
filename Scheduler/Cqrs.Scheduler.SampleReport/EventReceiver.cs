using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Chinchilla.StateManagement;
using Cqrs.Azure.Functions.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;

[ServiceBusAccount("ConnectionStrings:Cqrs.Azure.EventBus.ConnectionString")]
public class EventReceiver
{
	private readonly ILogger<EventReceiver> _logger;

	private readonly AzureFunctionEventBusReceiver<Guid> _eventReceiver;

	private readonly IContextItemCollection _backChannel;

	public EventReceiver(ILogger<EventReceiver> log, AzureFunctionEventBusReceiver<Guid> eventReceiver, IContextItemCollectionFactory contextItemCollectionFactory)
	{
		_logger = log;
		_eventReceiver = eventReceiver;
		_backChannel = contextItemCollectionFactory.GetCurrentContext();
	}

	/// <summary>
	/// Receives a <see cref="ServiceBusReceivedMessage"/> from the event bus.
	/// </summary>
	[FunctionName("SampleReports-Private")]
	public async virtual Task ReceiveEventPrivate([ServiceBusTrigger("%Cqrs:Azure:EventBus:PrivateEvent:Topic:Name%", "%Cqrs:Azure:EventBus:PrivateEvent:Topic:Subscription:Name%", AutoCompleteMessages = false)] ServiceBusReceivedMessage message, ServiceBusClient client, ServiceBusMessageActions messageActions, ExecutionContext executionContext)
	{
		_backChannel.SetData("Cqrs.Azure.FunctionName", executionContext?.FunctionName);
		await _eventReceiver.ReceiveEventAsync(client, messageActions, message);
	}
	/// <summary>
	/// Receives a <see cref="ServiceBusReceivedMessage"/> from the event bus.
	/// </summary>
	[FunctionName("SampleReports-Public")]
	public async virtual Task ReceiveEventPublic([ServiceBusTrigger("%Cqrs:Azure:EventBus:PublicEvent:Topic:Name%", "%Cqrs:Azure:EventBus:PublicEvent:Topic:Subscription:Name%", AutoCompleteMessages = false)] ServiceBusReceivedMessage message, ServiceBusClient client, ServiceBusMessageActions messageActions, ExecutionContext executionContext)
	{
		_backChannel.SetData("Cqrs.Azure.FunctionName", executionContext?.FunctionName);
		await _eventReceiver.ReceiveEventAsync(client, messageActions, message);
	}
}