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

namespace Cqrs.Authentication
{
	/// <summary>
	/// An authentication token with expiry and an issue time information.
	/// </summary>
	[Serializable]
	[DataContract]
	public class SingleSignOnToken : ISingleSignOnToken
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
		public virtual string Serialise()
		{
			return Token;
		}
	}
}