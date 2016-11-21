using System;
using cdmdotnet.AutoMapper;
using Cqrs.Entities;

namespace MyCompany.MyProject.Domain.Terminals.Events.Handlers
{
	public  partial class WithdrawValidatedEventHandler
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		public WithdrawValidatedEventHandler(IAutomapHelper automapHelper)
		{
			AutomapHelper = automapHelper;
		}

		partial void OnHandle(WithdrawValidated @event)
		{
			throw new NotImplementedException();
		}
	}
}
