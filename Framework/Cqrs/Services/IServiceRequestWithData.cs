namespace Cqrs.Services
{
	public interface IServiceRequestWithData<TAuthenticationToken, TData> : IServiceRequest<TAuthenticationToken>
	{
		TData Data { get; set; }
	}
}