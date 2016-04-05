using System;
using CQRSCode.ReadModel.Dtos;
using CQRSCode.ReadModel.Infrastructure;
using CQRSCode.WriteModel.Domain;
using Cqrs.Events;
using Cqrs.Authentication;

namespace CQRSCode.ReadModel.Handlers
{
	public class UserDetailView : IEventHandler<ISingleSignOnToken, DtoAggregateEvent<ISingleSignOnToken, UserDto>>
	{
		#region Implementation of IHandler<in DtoAggregateEvent<UserDto>>

		public void Handle(DtoAggregateEvent<ISingleSignOnToken, UserDto> message)
		{
			UserDetailsDto dto = null;
			switch (message.GetEventType())
			{
				case DtoAggregateEventType.Created:
					dto = new UserDetailsDto(message.Id, message.New.Name, message.Version);
					if (ReadModelFacade.UseSqlDatabase)
					{
						using (var datastore = new SqlDatabase())
							datastore.UserDetailsDtoStore.Add(dto);
					}
					else
						InMemoryDatabase.UserDetails.Add(message.Id, dto);
					break;
				case DtoAggregateEventType.Updated:
					if (ReadModelFacade.UseSqlDatabase)
					{
						using (var datastore = new SqlDatabase())
						{
							if (datastore.UserDetails.TryGetValue(message.Id, out dto))
							{
								dto.Name = message.New.Name;
								dto.Version = message.Version;
								datastore.UserDetailsDtoStore.Update(dto);
							}
						}
					}
					else
					{
						if (InMemoryDatabase.UserDetails.TryGetValue(message.Id, out dto))
							InMemoryDatabase.UserDetails[message.Id] = new UserDetailsDto(message.Id, message.New.Name, message.Version);
					}

					if (dto == null)
						throw new InvalidOperationException("Did not find the original item. This shouldn't happen.");
					break;
				case DtoAggregateEventType.Deleted:
					if (ReadModelFacade.UseSqlDatabase)
						using (var datastore = new SqlDatabase())
							if (datastore.UserDetails.TryGetValue(message.Id, out dto))
								datastore.UserDetailsDtoStore.Remove(dto);
					else if (InMemoryDatabase.UserDetails.TryGetValue(message.Id, out dto))
						InMemoryDatabase.UserDetails.Remove(message.Id);

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
