using System;
using cdmdotnet.AutoMapper;
using Cqrs.Entities;

namespace MyCompany.MyProject.Domain.Terminals.Events.Handlers
{
	public  partial class WithdrawFailedDueAmountLessThanZeroEventHandler
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		public WithdrawFailedDueAmountLessThanZeroEventHandler(IAutomapHelper automapHelper)
		{
			AutomapHelper = automapHelper;
		}

		partial void OnHandle(WithdrawFailedDueAmountLessThanZero @event)
		{
			throw new NotImplementedException();
		}
	}
}
