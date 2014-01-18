using Cqrs.Domain;

namespace Cqrs.Commands
{
	public class DtoCommandHandler<TPermissionScope, TDto> : ICommandHandler<TPermissionScope, DtoCommand<TPermissionScope, TDto>>
		where TDto : IDto
	{
		private IUnitOfWork<TPermissionScope> UnitOfWork { get; set; }

		public DtoCommandHandler(IUnitOfWork<TPermissionScope> unitOfWork)
		{
			UnitOfWork = unitOfWork;
		}

		#region Implementation of IHandler<in DtoCommand<UserDto>>

		public void Handle(DtoCommand<TPermissionScope, TDto> message)
		{
			var item = new DtoAggregateRoot<TPermissionScope, TDto>(message.Id, message.Original, message.New);
			UnitOfWork.Add(item);
			UnitOfWork.Commit();
		}

		#endregion
	}
}
