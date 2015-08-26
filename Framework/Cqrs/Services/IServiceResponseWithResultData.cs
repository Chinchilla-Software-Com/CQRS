using System.Runtime.Serialization;

namespace Cqrs.Services
{
	public interface IServiceResponseWithResultData<TResultData> : IServiceResponse
	{
		[DataMember]
		TResultData ResultData { get; set; }
	}
}