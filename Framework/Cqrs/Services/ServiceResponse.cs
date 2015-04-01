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
		public bool Success
		{
			get
			{
				return State == ServiceResponseStateType.Succeeded;
			}
		}

		[DataMember]
		public Guid CorrolationId { get; set; }
	}
}