using System;

namespace Cqrs.Services
{
	public interface IServiceResponseWithEventData : IServiceResponse
	{
		Guid CorrolationId { get; set; }
	}
}