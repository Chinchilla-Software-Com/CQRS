#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Cqrs.Authentication
{
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