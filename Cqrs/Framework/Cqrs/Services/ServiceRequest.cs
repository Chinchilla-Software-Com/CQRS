namespace Cqrs.Services
{
	public class ServiceRequest<TAuthenticationToken> : IServiceRequest<TAuthenticationToken>
	{
		public TAuthenticationToken AuthenticationToken { get; set; }
	}
}