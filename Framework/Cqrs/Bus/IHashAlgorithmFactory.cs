using System.Security.Cryptography;

namespace Cqrs.Bus
{
	/// <summary>
	/// A factory for creating new instances of <see cref="HashAlgorithm"/>, used for signing network messages.
	/// </summary>
	public interface IHashAlgorithmFactory
	{
		/// <summary>
		/// Creates a new instance of <see cref="HashAlgorithm"/>, used for signing network messages.
		/// </summary>
		HashAlgorithm Create();
	}
}