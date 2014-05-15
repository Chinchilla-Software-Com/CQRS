namespace Cqrs.Services
{
	public interface IVersionedServiceResponse : IServiceResponse
	{
		double Version { get; set; }
	}
}