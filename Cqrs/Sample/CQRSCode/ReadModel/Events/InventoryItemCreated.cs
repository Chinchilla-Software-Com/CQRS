using System;
using Cqrs.Events;
using Cqrs.Authentication;

namespace CQRSCode.ReadModel.Events
{
	public class InventoryItemCreated : IEvent<ISingleSignOnToken>
	{
		public readonly string Name;

		public InventoryItemCreated(Guid id, string name) 
		{
			Id = id;
			Name = name;
		}

		public Guid Id { get; set; }

		public int Version { get; set; }

		public DateTimeOffset TimeStamp { get; set; }

		#region Implementation of IMessageWithAuthenticationToken<ISingleSignOnToken>

		public ISingleSignOnToken AuthenticationToken { get; set; }

		#endregion
	}
}