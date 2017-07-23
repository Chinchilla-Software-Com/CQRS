#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Cqrs.Mongo.Entities;
using Cqrs.Authentication;

namespace Cqrs.Mongo.Repositories.Authentication
{
	[Serializable]
	[DataContract]
	public class SingleSignOnToken : MongoEntity, ISingleSignOnToken
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