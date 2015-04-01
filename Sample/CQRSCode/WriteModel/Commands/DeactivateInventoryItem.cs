using System;
using Cqrs.Commands;
using Cqrs.Authentication;

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

		#region Implementation of IMessageWithAuthenticationToken<ISingleSignOnToken>

		public ISingleSignOnToken AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IMessage

		public string CorrolationId { get; set; }

		#endregion
	}
}