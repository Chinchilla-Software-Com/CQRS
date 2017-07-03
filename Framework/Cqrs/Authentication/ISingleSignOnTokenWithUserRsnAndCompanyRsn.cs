#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Authentication
{
	/// <summary>
	/// This is a <see cref="ISingleSignOnTokenWithCompanyRsn"/> and <see cref="ISingleSignOnTokenWithUserRsn"/>
	/// </summary>
	public interface ISingleSignOnTokenWithUserRsnAndCompanyRsn : ISingleSignOnTokenWithCompanyRsn, ISingleSignOnTokenWithUserRsn
	{
	}
}