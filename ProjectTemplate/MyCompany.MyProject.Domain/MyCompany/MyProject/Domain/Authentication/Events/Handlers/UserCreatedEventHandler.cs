using System;
using cdmdotnet.AutoMapper;
using Cqrs.Entities;
using MyCompany.MyProject.Domain.Authentication.Repositories;

namespace MyCompany.MyProject.Domain.Authentication.Events.Handlers
{
	public  partial class UserCreatedEventHandler
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		protected IUserRepository UserRepository { get; private set; }

		public UserCreatedEventHandler(IAutomapHelper automapHelper, IUserRepository repository)
		{
			AutomapHelper = automapHelper;
			UserRepository = repository;
		}

		partial void OnHandle(UserCreated @event)
		{
			Entities.UserEntity entity = AutomapHelper.Automap<UserCreated, Entities.UserEntity>(@event);

			UserRepository.Create(entity);
		}
	}
}
