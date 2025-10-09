using Chinchilla.Logging;
using Cqrs.Commands;
using Microsoft.Azure.Functions.Worker;
using System;

namespace Cqrs.Azure.Functions.Isolated.Test;

public class TriggerScheduler
{
	private IAsyncCommandPublisher<Guid> CommandPublisher { get; set; }

	private ICorrelationIdHelper CorrelationIdHelper { get; set; }

	private ILogger Logger { get; set; }

	public TriggerScheduler(ILogger log, IAsyncCommandPublisher<Guid> commandPublisher, ICorrelationIdHelper correlationIdHelper)
	{
		Logger = log;
		CommandPublisher = commandPublisher;
		CorrelationIdHelper = correlationIdHelper;
	}

	[Function(nameof(TriggerScheduler))]
	public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
    {
		CorrelationIdHelper.SetCorrelationId(Guid.NewGuid());

		if (myTimer.ScheduleStatus != null)
		{
			Console.WriteLine($"Published for {myTimer.ScheduleStatus.Next}.");
			Logger.LogInfo($"Published for {myTimer.ScheduleStatus.Next}.");
		}

		await Task.CompletedTask;
		Environment.Exit(0);
	}
}