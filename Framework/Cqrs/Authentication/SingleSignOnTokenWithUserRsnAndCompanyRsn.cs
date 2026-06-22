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
	/// This is a <see cref="ISingleSignOnTokenWithCompanyRsn"/> and <see cref="ISingleSignOnTokenWithUserRsn"/>
	/// </summary>
	public class SingleSignOnTokenWithUserRsnAndCompanyRsn : SingleSignOnToken, ISingleSignOnTokenWithUserRsnAndCompanyRsn
	{
		/// <summary>
		/// The Rsn of the company the user doing the operation is operating on.
		/// When used in a system where a single user can have access to multiple companies, this is not the company the user belongs to, but the company it is operating on.
		/// When used by an external 3rd party this is the all in context of the person being impersonated, not the 3rd party system itself.
		/// </summary>
		[Required]
		[DataMember]
		public Guid CompanyRsn { get; set; }

		/// <summary>
		/// The Rsn of the user doing the operation. When used by an external 3rd party this is the person being impersonated, not the 3rd party system itself.
		/// </summary>
		[Required]
		[DataMember]
		public Guid UserRsn { get; set; }

		/// <summary>
		/// Returns <see cref="CompanyRsn"/> and <see cref="UserRsn"/>.
		/// </summary>
		/// <returns><see cref="CompanyRsn"/> and <see cref="UserRsn"/>.</returns>
		public override string Serialise()
		{
			return string.Format("{0:N}/{1:N}", CompanyRsn, UserRsn);
		}
	}
}