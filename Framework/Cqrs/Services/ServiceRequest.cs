using System.Runtime.Serialization;

namespace Cqrs.Services
{
	public class ServiceRequest<TAuthenticationToken> : IServiceRequest<TAuthenticationToken>
	{
		[DataMember]
		public TAuthenticationToken AuthenticationToken { get; set; }
	}
}