using System;
using System.Runtime.Serialization;

namespace Cqrs.Services
{
	[Serializable]
	[DataContract]
	public class ServiceResponseWithResultData<TResultData> : ServiceResponse, IServiceResponseWithResultData<TResultData>, IVersionedServiceResponse
	{
		[DataMember]
		public TResultData ResultData { get; set; }

		public ServiceResponseWithResultData()
		{
		}

		public ServiceResponseWithResultData(TResultData resultData)
		{
			State = ServiceResponseStateType.Succeeded;
			ResultData = resultData;
		}

		[DataMember]
		public double Version { get; set; }
	}
}