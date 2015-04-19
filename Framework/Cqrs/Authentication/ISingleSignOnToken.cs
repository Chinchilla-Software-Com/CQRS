using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Cqrs.Authentication
{
	[ServiceContract(Namespace = "http://cqrs.co.nz/SingleSignOn/Token")]
	public interface ISingleSignOnToken
	{
		[Required]
		[DataMember]
		string Token { get; set; }

		[Required]
		[DataMember]
		DateTime TimeOfExpiry { get; set; }

		[Required]
		[DataMember]
		DateTime DateIssued { get; set; }
	}
}