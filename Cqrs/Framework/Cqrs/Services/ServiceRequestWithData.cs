namespace Cqrs.Services
{
	public class ServiceRequestWithData<TAuthenticationToken, TData> : ServiceRequest<TAuthenticationToken>, IServiceRequestWithData<TAuthenticationToken, TData>
	{
		public TData Data { get; set; }
	}
}