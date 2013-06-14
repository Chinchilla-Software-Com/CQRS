using System;
using CQRSCode.ReadModel.Dtos;
using CQRSCode.ReadModel.Infrastructure;
using CQRSCode.WriteModel.Domain;
using Cqrs.Events;

namespace CQRSCode.ReadModel.Handlers
{
	public class UserDetailView : IEventHandler<DtoAggregateEvent<UserDto>>	
	{
		#region Implementation of IHandler<in DtoAggregateEvent<UserDto>>

		public void Handle(DtoAggregateEvent<UserDto> message)
		{
			switch (message.GetEventType())
			{
				case DtoAggregateEventType.Created:
					InMemoryDatabase.UserDetails.Add(message.Id, new UserDetailsDto(message.Id, message.New.Name, message.Version));
					break;
				case DtoAggregateEventType.Updated:
					if (!InMemoryDatabase.UserDetails.ContainsKey(message.Id))
						throw new InvalidOperationException("Did not find the original item. This shouldn't happen.");
					InMemoryDatabase.UserDetails[message.Id] = new UserDetailsDto(message.Id, message.New.Name, message.Version);
					break;
				case DtoAggregateEventType.Deleted:
					if (!InMemoryDatabase.UserDetails.ContainsKey(message.Id))
						throw new InvalidOperationException("Did not find the original item. This shouldn't happen.");
					InMemoryDatabase.UserDetails.Remove(message.Id);
					break;
				default:
					throw new InvalidOperationException("Unknown event. This shouldn't happen.");
			}
		}

		#endregion
	}
}
