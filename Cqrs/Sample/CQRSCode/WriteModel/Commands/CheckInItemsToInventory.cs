using System;
using Cqrs.Commands;
using Cqrs.Repositories.Authentication;

namespace CQRSCode.WriteModel.Commands
{
	public class CheckInItemsToInventory : ICommand<ISingleSignOnToken>
	{
		public readonly int Count;

		public CheckInItemsToInventory(Guid id, int count, int originalVersion) 
		{
			Id = id;
			Count = count;
			ExpectedVersion = originalVersion;
		}

		public Guid Id { get; set; }

		public int ExpectedVersion { get; set; }

		#region Implementation of IMessageWithAuthenticationToken<ISingleSignOnToken>

		public ISingleSignOnToken AuthenticationToken { get; set; }

		#endregion
	}
}