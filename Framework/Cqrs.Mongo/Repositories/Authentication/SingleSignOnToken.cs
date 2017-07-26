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
	/// <summary>
	/// An <see cref="ISingleSignOnToken"/> designed to work with MongoDB.
	/// </summary>
	[Serializable]
	[DataContract]
	public class SingleSignOnToken : MongoEntity, ISingleSignOnToken
	{
		/// <summary>
		/// The authentication token.
		/// </summary>
		[Required]
		[DataMember]
		public string Token { get; set; }

		/// <summary>
		/// The <see cref="DateTime"/> this token should expire.
		/// </summary>
		[Required]
		[DataMember]
		public DateTime TimeOfExpiry { get; set; }

		/// <summary>
		/// The <see cref="DateTime"/> this token was issued.
		/// </summary>
		[Required]
		[DataMember]
		public DateTime DateIssued { get; set; }

		/// <summary>
		/// Returns <see cref="Token"/>.
		/// </summary>
		/// <returns><see cref="Token"/>.</returns>
		public string Serialise()
		{
			return Token;
		}
	}
}