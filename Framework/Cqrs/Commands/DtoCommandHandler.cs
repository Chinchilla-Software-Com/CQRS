#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Threading.Tasks;
using Cqrs.Domain;

namespace Cqrs.Commands
{
	/// <summary>
	/// A <see cref="ICommandHandle"/> for working with <see cref="DtoCommand{TAuthenticationToken,TDto}"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	/// <typeparam name="TDto">The <see cref="Type"/> of <see cref="IDto"/> this command targets.</typeparam>
	public class DtoCommandHandler<TAuthenticationToken, TDto> : ICommandHandler<TAuthenticationToken, DtoCommand<TAuthenticationToken, TDto>>
		where TDto : IDto
	{
		private IUnitOfWork<TAuthenticationToken> UnitOfWork { get; set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="DtoCommandHandler{TAuthenticationToken,TDto}"/>
		/// </summary>
		public DtoCommandHandler(IUnitOfWork<TAuthenticationToken> unitOfWork)
		{
			UnitOfWork = unitOfWork;
		}

		#region Implementation of IMessageHandler<in DtoCommand<UserDto>>

		/// <summary>
		/// Responds to the provided <paramref name="message"/>.
		/// </summary>
		/// <param name="message">The <see cref="DtoCommand{TAuthenticationToken,TDto}"/> to respond to or "handle"</param>
		public virtual
#if NET40
			void Handle
#else
			async Task HandleAsync
#endif
			(DtoCommand<TAuthenticationToken, TDto> message)
		{
			var item = new DtoAggregateRoot<TAuthenticationToken, TDto>(message.Id, message.Original, message.New);

#if NET40
			UnitOfWork.Add(item);
			UnitOfWork.Commit();
#else
			await UnitOfWork.AddAsync(item);
			await UnitOfWork.CommitAsync();
#endif
		}

		#endregion
	}
}
