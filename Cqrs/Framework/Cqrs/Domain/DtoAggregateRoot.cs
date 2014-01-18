using System;
using Cqrs.Events;

namespace Cqrs.Domain
{
	public class DtoAggregateRoot<TPermissionScope, TDto> : AggregateRoot<TPermissionScope>
		where TDto : IDto
	{
		public DtoAggregateRoot(Guid id, TDto original, TDto @new)
		{
			Id = id;
			ApplyChange(new DtoAggregateEvent<TPermissionScope, TDto>(id, original, @new));
		}
	}
}