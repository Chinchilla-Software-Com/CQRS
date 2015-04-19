using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Cqrs.Authentication;

namespace Cqrs.Repositories.Authentication
{
	[Serializable]
	[DataContract]
	public class SingleSignOnToken : ISingleSignOnToken
	{
		[Required]
		[DataMember]
		public string Token { get; set; }

		[Required]
		[DataMember]
		public DateTime TimeOfExpiry { get; set; }

		[Required]
		[DataMember]
		public DateTime DateIssued { get; set; }
	}
}