using System;
using Cqrs.Events;
using Cqrs.Authentication;

namespace CQRSCode.ReadModel.Events
{
	public class ItemsRemovedFromInventory : IEvent<ISingleSignOnToken>
	{
		public readonly int Count;
 
		public ItemsRemovedFromInventory(Guid id, int count) 
		{
			Id = id;
			Count = count;
		}

		public Guid Id { get; set; }

		public int Version { get; set; }

		public DateTimeOffset TimeStamp { get; set; }

		#region Implementation of IMessageWithPermissionToken<ISingleSignOnToken>

		public ISingleSignOnToken AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IMessage

		public Guid CorrolationId { get; set; }

		#endregion
	}
}