namespace Cqrs.Services
{
	public interface IServiceRequest<TAuthenticationToken>
	{
		TAuthenticationToken AuthenticationToken { get; set; }
	}
}