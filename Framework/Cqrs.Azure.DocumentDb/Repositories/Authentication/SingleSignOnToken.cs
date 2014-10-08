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
using Cqrs.Authentication;

namespace Cqrs.Azure.DocumentDb.Repositories.Authentication
{
	[Serializable]
	[DataContract]
	public class SingleSignOnToken : Entities.Entity, ISingleSignOnToken
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