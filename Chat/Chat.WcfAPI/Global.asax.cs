namespace Chat.WcfAPI
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

		#region Overrides of CqrsHttpApplication

		/// <summary>
		/// Call the static "RegisterDataContracts" method on any <see cref="T:Cqrs.Services.IServiceParameterResolver"/> we can find in the <see cref="T:System.Reflection.Assembly"/> of any <see cref="T:System.Type"/> in <see cref="P:Cqrs.Hosts.CqrsHttpApplication.HandlerTypes"/>
		/// </summary>
		protected override void RegisterServiceParameterResolver()
		{
			base.RegisterServiceParameterResolver();
		}

		#endregion
	}
}