using System;
using Cqrs.Commands;
using Cqrs.Repositories.Authentication;

namespace CQRSCode.WriteModel.Commands
{
	public class RenameInventoryItem : ICommand<ISingleSignOnToken>
	{
		public readonly string NewName;

		public RenameInventoryItem(Guid id, string newName, int originalVersion)
		{
			Id = id;
			NewName = newName;
			ExpectedVersion = originalVersion;
		}

		public Guid Id { get; set; }

		public int ExpectedVersion { get; set; }

		#region Implementation of IMessageWithAuthenticationToken<ISingleSignOnToken>

		public ISingleSignOnToken AuthenticationToken { get; set; }

		#endregion
	}
}