using System;
using cdmdotnet.AutoMapper;
using Cqrs.Entities;
using MyCompany.MyProject.Domain.Authentication.Repositories;

namespace MyCompany.MyProject.Domain.Authentication.Events.Handlers
{
	public  partial class UserDeletedEventHandler
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		protected IUserRepository UserRepository { get; private set; }

		public UserDeletedEventHandler(IAutomapHelper automapHelper, IUserRepository repository)
		{
			AutomapHelper = automapHelper;
			UserRepository = repository;
		}

		partial void OnHandle(UserDeleted @event)
		{
			Entities.UserEntity entity = UserRepository.Load(@event.Rsn);;
			entity.IsLogicallyDeleted = true;

			UserRepository.Update(entity);
		}
	}
}
