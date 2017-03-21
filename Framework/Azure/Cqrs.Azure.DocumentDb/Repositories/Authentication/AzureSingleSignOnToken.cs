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
using Cqrs.Azure.DocumentDb.Entities;

namespace Cqrs.Azure.DocumentDb.Repositories.Authentication
{
	[Serializable]
	[DataContract]
	public class AzureSingleSignOnToken : AzureDocumentDbEntity, ISingleSignOnToken
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

		public string Serialise()
		{
			return Token;
		}
	}
}