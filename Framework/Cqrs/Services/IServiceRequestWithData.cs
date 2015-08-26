using System.Runtime.Serialization;

namespace Cqrs.Services
{
	public interface IServiceRequestWithData<TAuthenticationToken, TData> : IServiceRequest<TAuthenticationToken>
	{
		[DataMember]
		TData Data { get; set; }
	}
}