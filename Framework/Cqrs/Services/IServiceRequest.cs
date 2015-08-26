using System;
using System.Runtime.Serialization;

namespace Cqrs.Services
{
	public interface IServiceRequest<TAuthenticationToken>
	{
		[DataMember]
		TAuthenticationToken AuthenticationToken { get; set; }

		[DataMember]
		Guid CorrelationId { get; set; }
	}
}