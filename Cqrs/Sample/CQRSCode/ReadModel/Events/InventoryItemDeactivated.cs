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

		#region Implementation of IMessageWithPermissionToken<ISingleSignOnToken>

		public ISingleSignOnToken PermissionToken { get; set; }

		#endregion
	}
}