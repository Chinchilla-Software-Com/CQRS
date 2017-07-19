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
using Cqrs.DataStores;

namespace Cqrs.Authentication
{
	/// <summary>
	/// This is a <see cref="ISingleSignOnToken"/> that includes an identifiable <see cref="CompanyRsn"/> to optimise the hits of the <see cref="IDataStore{TData}">DataStores</see> by including data you most likely need.
	/// As such, if not used correctly, this can expose identifiable information.
	/// It is suggested the service layer populates this before sending commands as part of authorisation/authentication.
	/// </summary>
	public class SingleSignOnTokenWithCompanyRsn : SingleSignOnToken
	{
		/// <summary>
		/// The Rsn of the company the user doing the operation is operating on.
		/// When used in a system where a single user can have access to multiple companies, this is not the company the user belongs to, but the company it is operating on.
		/// When used by an external 3rd party this is the all in context of the person being impersonated, not the 3rd party system itself.
		/// </summary>
		[Required]
		[DataMember]
		public Guid CompanyRsn { get; set; }

		public override string Serialise()
		{
			return CompanyRsn.ToString("N");
		}
	}
}