using System;
using System.Runtime.Serialization;

namespace Cqrs.Services
{
	[Serializable]
	[DataContract]
	public class ServiceRequestWithData<TAuthenticationToken, TData> : ServiceRequest<TAuthenticationToken>, IServiceRequestWithData<TAuthenticationToken, TData>
	{
		[DataMember]
		public TData Data { get; set; }
	}
}