using System;
using cdmdotnet.AutoMapper;
using Cqrs.Entities;

namespace MyCompany.MyProject.Domain.Terminals.Events.Handlers
{
	public  partial class WithdrawFailedDueInsufficientFundsEventHandler
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		public WithdrawFailedDueInsufficientFundsEventHandler(IAutomapHelper automapHelper)
		{
			AutomapHelper = automapHelper;
		}

		partial void OnHandle(WithdrawFailedDueInsufficientFunds @event)
		{
			throw new NotImplementedException();
		}
	}
}
