using System;
using System.Runtime.Serialization;

namespace Cqrs.Services
{
	[Serializable]
	[DataContract]
	public class ServiceRequest<TAuthenticationToken> : IServiceRequest<TAuthenticationToken>
	{
		[DataMember]
		public TAuthenticationToken AuthenticationToken { get; set; }
	}
}