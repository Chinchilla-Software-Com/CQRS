using System;
using Cqrs.Events;
using Cqrs.Repositories.Authentication;

namespace CQRSCode.ReadModel.Events
{
	public class InventoryItemDeactivated : IEvent<ISingleSignOnToken>
	{
		public InventoryItemDeactivated(Guid id)
		{
			Id = id;
		}

		public Guid Id { get; set; }

		public int Version { get; set; }

		public DateTimeOffset TimeStamp { get; set; }

		#region Implementation of IMessageWithAuthenticationToken<ISingleSignOnToken>

		public ISingleSignOnToken AuthenticationToken { get; set; }

		#endregion
	}
}