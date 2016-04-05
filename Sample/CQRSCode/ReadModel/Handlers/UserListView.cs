using System;
using System.Linq;
using CQRSCode.ReadModel.Dtos;
using CQRSCode.ReadModel.Infrastructure;
using CQRSCode.WriteModel.Domain;
using Cqrs.Events;
using Cqrs.Authentication;

namespace CQRSCode.ReadModel.Handlers
{
	public class UserListView : IEventHandler<ISingleSignOnToken, DtoAggregateEvent<ISingleSignOnToken, UserDto>>
	{
		#region Implementation of IHandler<in DtoAggregateEvent<UserDto>>

		public void Handle(DtoAggregateEvent<ISingleSignOnToken, UserDto> message)
		{
			UserListDto dto;
			switch (message.GetEventType())
			{
				case DtoAggregateEventType.Created:
					dto = new UserListDto(message.Id, message.New.Name);
					if (ReadModelFacade.UseSqlDatabase)
					{
						using (var datastore = new SqlDatabase())
							datastore.UserListDtoStore.Add(dto);
					}
					else
						InMemoryDatabase.UserList.Add(dto);
					break;
				case DtoAggregateEventType.Updated:
					if (ReadModelFacade.UseSqlDatabase)
					{
						using (var datastore = new SqlDatabase())
						{
							dto = datastore.UserList.SingleOrDefault(x => x.Id == message.Id);
							if (dto != null)
							{
								dto.Name = message.New.Name;
								datastore.UserListDtoStore.Update(dto);
							}
						}
					}
					else
					{
						dto = InMemoryDatabase.UserList.SingleOrDefault(x => x.Id == message.Id);
						if (dto != null)
						{
							InMemoryDatabase.UserList.Remove(dto);
							InMemoryDatabase.UserList.Add(new UserListDto(message.Id, message.New.Name));
						}
					}

					if (dto == null)
						throw new InvalidOperationException("Did not find the original item. This shouldn't happen.");
					break;
				case DtoAggregateEventType.Deleted:
					if (ReadModelFacade.UseSqlDatabase)
					{
						using (var datastore = new SqlDatabase())
						{
							dto = datastore.UserList.SingleOrDefault(x => x.Id == message.Id);
							if (dto != null)
							{
								dto.Name = message.New.Name;
								datastore.UserListDtoStore.Remove(dto);
							}
						}
					}
					else
					{
						dto = InMemoryDatabase.UserList.SingleOrDefault(x => x.Id == message.Id);
						if (dto != null)
						{
							InMemoryDatabase.UserList.Remove(dto);
						}
					}

					if (dto == null)
						throw new InvalidOperationException("Did not find the original item. This shouldn't happen.");
					break;
				default:
					throw new InvalidOperationException("Unknown event. This shouldn't happen.");
			}
		}

		#endregion
	}
}