using System;
using System.Runtime.Serialization;

namespace Cqrs.Services
{
	[Serializable]
	[DataContract]
	public class ServiceResponseWithEventData : ServiceResponse, IServiceResponseWithEventData
	{
		[DataMember]
		public Guid CorrolationId { get; set; }

		public ServiceResponseWithEventData()
		{
		}

		public ServiceResponseWithEventData(Guid corrolationId)
		{
			State = ServiceResponseStateType.Succeeded;
			CorrolationId = corrolationId;
		}

		public ServiceResponseWithEventData(ServiceResponseStateType state, Guid corrolationId)
		{
			State = state;
			CorrolationId = corrolationId;
		}

		public ServiceResponseWithEventData(ServiceResponseStateType state)
		{
			State = state;
		}
	}
}