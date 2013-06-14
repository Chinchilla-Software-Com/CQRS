using System;
using Cqrs.Events;

namespace Cqrs.Domain
{
	public class DtoAggregateRoot<TDto> : AggregateRoot
		where TDto : IDto
	{
		public DtoAggregateRoot(Guid id, TDto original, TDto @new)
		{
			Id = id;
			ApplyChange(new DtoAggregateEvent<TDto>(id, original, @new));
		}
	}
}