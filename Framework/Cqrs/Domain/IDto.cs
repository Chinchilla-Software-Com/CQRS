using System;

namespace Cqrs.Domain
{
	public interface IDto
	{
		Guid Id { get; set; }
	}
}