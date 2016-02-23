using System;
using cdmdotnet.AutoMapper;
using Cqrs.Entities;
using MyCompany.MyProject.Domain.Authentication.Repositories;

namespace MyCompany.MyProject.Domain.Authentication.Events.Handlers
{
	public  partial class UserUpdatedEventHandler
	{
		protected IAutomapHelper AutomapHelper { get; private set; }

		protected IUserRepository UserRepository { get; private set; }

		public UserUpdatedEventHandler(IAutomapHelper automapHelper, IUserRepository repository)
		{
			AutomapHelper = automapHelper;
			UserRepository = repository;
		}

		partial void OnHandle(UserUpdated @event)
		{
			Entities.UserEntity entity = UserRepository.Load(@event.Rsn);;
			entity = AutomapHelper.Automap(@event, entity);

			UserRepository.Update(entity);
		}
	}
}
