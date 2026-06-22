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
	public interface ISingleSignOnToken
	{
		/// <summary>
		/// The authentication token.
		/// </summary>
		[Required]
		[DataMember]
		string Token { get; set; }

		/// <summary>
		/// The <see cref="DateTime"/> this token should expire.
		/// </summary>
		[Required]
		[DataMember]
		DateTime TimeOfExpiry { get; set; }

		/// <summary>
		/// The <see cref="DateTime"/> this token was issued.
		/// </summary>
		[Required]
		[DataMember]
		DateTime DateIssued { get; set; }

		/// <summary>
		/// Serialises this token to a string.
		/// </summary>
		string Serialise();
	}
}