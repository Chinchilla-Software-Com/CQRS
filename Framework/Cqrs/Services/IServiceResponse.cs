using System;

namespace Cqrs.Services
{
	public interface IServiceResponse
	{
		ServiceResponseStateType State { get; set; }

		bool Success { get; }

		Guid CorrolationId { get; set; }
	}
}