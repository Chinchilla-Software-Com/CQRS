using Cqrs.Domain;

namespace Cqrs.Commands
{
	public class DtoCommandHandler<TPermissionToken, TDto> : ICommandHandler<TPermissionToken, DtoCommand<TPermissionToken, TDto>>
		where TDto : IDto
	{
		private IUnitOfWork<TPermissionToken> UnitOfWork { get; set; }

		public DtoCommandHandler(IUnitOfWork<TPermissionToken> unitOfWork)
		{
			UnitOfWork = unitOfWork;
		}

		#region Implementation of IHandler<in DtoCommand<UserDto>>

		public void Handle(DtoCommand<TPermissionToken, TDto> message)
		{
			var item = new DtoAggregateRoot<TPermissionToken, TDto>(message.Id, message.Original, message.New);
			UnitOfWork.Add(item);
			UnitOfWork.Commit();
		}

		#endregion
	}
}
