using System;
using Cqrs.Events;

namespace Cqrs.Domain
{
	public class DtoAggregateRoot<TPermissionToken, TDto> : AggregateRoot<TPermissionToken>
		where TDto : IDto
	{
		public DtoAggregateRoot(Guid id, TDto original, TDto @new)
		{
			Id = id;
			ApplyChange(new DtoAggregateEvent<TPermissionToken, TDto>(id, original, @new));
		}
	}
}