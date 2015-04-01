using System.Runtime.Serialization;

namespace Cqrs.Services
{
	public class ServiceRequestWithData<TAuthenticationToken, TData> : ServiceRequest<TAuthenticationToken>, IServiceRequestWithData<TAuthenticationToken, TData>
	{
		[DataMember]
		public TData Data { get; set; }
	}
}