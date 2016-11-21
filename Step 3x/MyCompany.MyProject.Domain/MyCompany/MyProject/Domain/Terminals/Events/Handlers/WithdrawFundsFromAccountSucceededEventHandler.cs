using System;
using cdmdotnet.AutoMapper;
using Cqrs.Entities;

namespace MyCompany.MyProject.Domain.Terminals.Events.Handlers
{
	public  partial class WithdrawFundsFromAccountSucceededEventHandler
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		public WithdrawFundsFromAccountSucceededEventHandler(IAutomapHelper automapHelper)
		{
			AutomapHelper = automapHelper;
		}

		partial void OnHandle(WithdrawFundsFromAccountSucceeded @event)
		{
			throw new NotImplementedException();
		}
	}
}
