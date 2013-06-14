using Cqrs.Domain;

namespace Cqrs.Commands
{
	public class DtoCommandHandler<TDto> : ICommandHandler<DtoCommand<TDto>>
		where TDto : IDto
	{
		private IUnitOfWork UnitOfWork { get; set; }

		public DtoCommandHandler(IUnitOfWork unitOfWork)
		{
			UnitOfWork = unitOfWork;
		}

		#region Implementation of IHandler<in DtoCommand<UserDto>>

		public void Handle(DtoCommand<TDto> message)
		{
			var item = new DtoAggregateRoot<TDto>(message.Id, message.Original, message.New);
			UnitOfWork.Add(item);
			UnitOfWork.Commit();
		}

		#endregion
	}
}
