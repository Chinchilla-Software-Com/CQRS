#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Configuration
{
	/// <summary>
	/// Provides access to secrets such as keys or credentials
	/// </summary>
	public interface ISecretFactory
	{
		/// <summary>
		/// Get the specified secret as identified by the provided <paramref name="secretName"/>.
		/// </summary>
		/// <param name="secretName">The name of the secret.</param>
		/// <returns>The secret</returns>
		string GetSecret(string secretName);
	}
}