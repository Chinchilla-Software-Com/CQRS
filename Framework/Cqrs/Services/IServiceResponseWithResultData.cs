namespace Cqrs.Services
{
	public interface IServiceResponseWithResultData<TResultData> : IServiceResponse
	{
		TResultData ResultData { get; set; }
	}
}