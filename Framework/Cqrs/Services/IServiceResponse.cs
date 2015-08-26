using System;
using System.Runtime.Serialization;

namespace Cqrs.Services
{
	public interface IServiceResponse
	{
		[DataMember]
		ServiceResponseStateType State { get; set; }

		[DataMember]
		Guid CorrelationId { get; set; }
	}
}