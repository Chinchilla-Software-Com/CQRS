#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Security.Cryptography;

namespace Cqrs.Bus
{
	/// <summary>
	/// A factory for creating new instances of <see cref="HashAlgorithm"/>, used for signing network messages.
	/// </summary>
	public class BuiltInHashAlgorithmFactory : IHashAlgorithmFactory
	{
		/// <summary>
		/// Creates a new instance of <see cref="HashAlgorithm"/> using <see cref="SHA512.Create()"/>.
		/// </summary>
		public HashAlgorithm Create()
		{
			return SHA512.Create();
		}
	}
}