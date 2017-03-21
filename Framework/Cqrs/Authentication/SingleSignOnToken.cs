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

		public virtual string Serialise()
		{
			return Token;
		}
	}
}