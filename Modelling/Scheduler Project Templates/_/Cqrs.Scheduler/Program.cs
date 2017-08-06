namespace $safeprojectname$
{
	/// <summary>
	/// Starts the Scheduler Webjob
	/// </summary>
	public class SchedulerWebJob
	{
		/// <remarks>
		/// Please set the following connection strings in app.config for this WebJob to run:
		/// AzureWebJobsDashboard and AzureWebJobsStorage
		/// Better yet, add them to your Azure portal so they can be changed at runtime without re-deploying or re-compiling.
		/// </remarks>
		public static void Main()
		{
			var scheduler = new Scheduler();
			scheduler.Run();
			scheduler.WhatTimeIsIt();
		}
	}
}