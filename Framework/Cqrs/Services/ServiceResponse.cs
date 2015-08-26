using System;
using System.Runtime.Serialization;

namespace Cqrs.Services
{
	[Serializable]
	[DataContract]
	public class ServiceResponse : IServiceResponse
	{
		public ServiceResponse()
		{
			State = ServiceResponseStateType.Succeeded;
		}

		[DataMember]
		public ServiceResponseStateType State { get; set; }

		[DataMember]
		public Guid CorrelationId { get; set; }
	}
}