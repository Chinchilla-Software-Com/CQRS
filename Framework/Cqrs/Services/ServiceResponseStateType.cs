using System;
using System.Runtime.Serialization;

namespace Cqrs.Services
{
	[Serializable]
	[DataContract]
	public enum ServiceResponseStateType
	{
		[EnumMember]
		Unknown = 0,

		[EnumMember]
		Succeeded = 1,

		[EnumMember]
		FailedWithAFatalException = 2,

		[EnumMember]
		FailedWithAnUnexpectedException = 3,

		[EnumMember]
		FailedAuthentication = 11,

		[EnumMember]
		FailedAuthorisation = 12,

		[EnumMember]
		FailedValidation = 21
	}
}