using System;
using System.Linq;
using CQRSCode.ReadModel.Dtos;
using CQRSCode.ReadModel.Infrastructure;
using CQRSCode.WriteModel.Domain;
using Cqrs.Events;

namespace CQRSCode.ReadModel.Handlers
{
	public class UserListView : IEventHandler<DtoAggregateEvent<UserDto>>
	{
		#region Implementation of IHandler<in DtoAggregateEvent<UserDto>>

		public void Handle(DtoAggregateEvent<UserDto> message)
		{
			UserListDto existingUser;
			switch (message.GetEventType())
			{
				case DtoAggregateEventType.Created:
					InMemoryDatabase.UserList.Add(new UserListDto(message.Id, message.New.Name));
					break;
				case DtoAggregateEventType.Updated:
					existingUser = InMemoryDatabase.UserList.SingleOrDefault(user => user.Id == message.Id);
					if (existingUser == null)
						throw new InvalidOperationException("Did not find the original item. This shouldn't happen.");
					InMemoryDatabase.UserList.Remove(existingUser);
					InMemoryDatabase.UserList.Add(new UserListDto(message.Id, message.New.Name));
					break;
				case DtoAggregateEventType.Deleted:
					existingUser = InMemoryDatabase.UserList.SingleOrDefault(user => user.Id == message.Id);
					if (existingUser == null)
						throw new InvalidOperationException("Did not find the original item. This shouldn't happen.");
					InMemoryDatabase.UserList.Remove(existingUser);
					break;
				default:
					throw new InvalidOperationException("Unknown event. This shouldn't happen.");
			}
		}

		#endregion
	}
}