#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Domain;

namespace Cqrs.Commands
{
	public class DtoCommandHandler<TAuthenticationToken, TDto> : ICommandHandler<TAuthenticationToken, DtoCommand<TAuthenticationToken, TDto>>
		where TDto : IDto
	{
		private IUnitOfWork<TAuthenticationToken> UnitOfWork { get; set; }

		public DtoCommandHandler(IUnitOfWork<TAuthenticationToken> unitOfWork)
		{
			UnitOfWork = unitOfWork;
		}

		#region Implementation of IMessageHandler<in DtoCommand<UserDto>>

		public void Handle(DtoCommand<TAuthenticationToken, TDto> message)
		{
			var item = new DtoAggregateRoot<TAuthenticationToken, TDto>(message.Id, message.Original, message.New);
			UnitOfWork.Add(item);
			UnitOfWork.Commit();
		}

		#endregion
	}
}
