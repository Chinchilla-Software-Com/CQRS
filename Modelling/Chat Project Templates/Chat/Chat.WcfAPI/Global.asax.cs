namespace $safeprojectname$
{
	using Cqrs.Hosts;
	using MicroServices.Conversations.Commands;
	using System;

	public class Global : CqrsHttpApplication<Guid>
	{
		public Global()
		{
			HandlerTypes = new[] {typeof (StartConversation)};
		}
	}
}