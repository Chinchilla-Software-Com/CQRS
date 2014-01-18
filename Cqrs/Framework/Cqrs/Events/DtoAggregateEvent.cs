using System;
using Cqrs.Domain;

namespace Cqrs.Events
{
	public class DtoAggregateEvent<TPermissionToken, TDto> : IEvent<TPermissionToken>
		where TDto : IDto
	{
		public TDto Original { get; private set; }

		public TDto New { get; private set; }

		public DtoAggregateEvent(Guid id, TDto original, TDto @new)
		{
			Id = id;
			Original = original;
			New = @new;
		}

		public Guid Id { get; set; }

		public int Version { get; set; }

		public DateTimeOffset TimeStamp { get; set; }

		public DtoAggregateEventType GetEventType()
		{
			if (New != null && Original == null)
				return DtoAggregateEventType.Created;
			if (New != null && Original != null)
				return DtoAggregateEventType.Updated;
			if (New == null && Original != null)
				return DtoAggregateEventType.Deleted;
			return DtoAggregateEventType.Unknown;
		}

		#region Implementation of IMessageWithPermissionToken<TPermissionToken>

		public TPermissionToken PermissionToken { get; set; }

		#endregion
	}
}