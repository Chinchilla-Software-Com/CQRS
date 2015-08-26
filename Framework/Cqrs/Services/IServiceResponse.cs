using System;

namespace Cqrs.Services
{
	public interface IServiceResponse
	{
		ServiceResponseStateType State { get; set; }

		Guid CorrelationId { get; set; }
	}
}