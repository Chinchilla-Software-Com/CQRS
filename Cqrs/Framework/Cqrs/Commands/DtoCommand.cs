using System;
using Cqrs.Domain;

namespace Cqrs.Commands
{
	/// <summary>
	/// A <see cref="ICommand"/> for <see cref="IDto"/> objects
	/// </summary>
	public class DtoCommand<TDto> : ICommand
		where TDto : IDto
	{
		public TDto Original { get; set; }
		public TDto New { get; set; }

		public DtoCommand(Guid id, TDto original, TDto @new)
		{
			Id = id;
			Original = original;
			New = @new;
		}

		public Guid Id { get; set; }
		public int ExpectedVersion { get; set; }
	}
}