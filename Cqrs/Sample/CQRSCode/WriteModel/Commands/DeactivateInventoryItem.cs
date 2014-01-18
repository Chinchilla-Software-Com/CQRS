using System;
using Cqrs.Commands;
using Cqrs.Repositories.Authentication;

namespace CQRSCode.WriteModel.Commands
{
	public class DeactivateInventoryItem : ICommand<ISingleSignOnToken>
	{
		public DeactivateInventoryItem(Guid id, int originalVersion)
		{
			Id = id;
			ExpectedVersion = originalVersion;
		}

		public Guid Id { get; set; }

		public int ExpectedVersion { get; set; }

		#region Implementation of IMessageWithPermissionToken<ISingleSignOnToken>

		public ISingleSignOnToken PermissionToken { get; set; }

		#endregion
	}
}