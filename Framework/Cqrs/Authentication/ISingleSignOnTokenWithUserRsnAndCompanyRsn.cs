#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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