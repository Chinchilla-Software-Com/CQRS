#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Domain.Exceptions
{
	/// <summary>
	/// The <see cref="ISaga{TAuthenticationToken}"/> requested was not found.
	/// </summary>
	/// <typeparam name="TSaga">The <see cref="Type"/> of the <see cref="ISaga{TAuthenticationToken}"/> that wasn't found.</typeparam>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	[Serializable]
	public class SagaNotFoundException<TSaga, TAuthenticationToken> : Exception
		where TSaga : ISaga<TAuthenticationToken>
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="SagaNotFoundException{TSaga,TAuthenticationToken}"/> with the provided identifier of the <see cref="ISaga{TAuthenticationToken}"/> that wasn't found.
		/// </summary>
		/// <param name="id">The identifier of the <see cref="ISaga{TAuthenticationToken}"/> that wasn't found.</param>
		public SagaNotFoundException(Guid id)
			: base(string.Format("Saga '{0}' of type '{1}' was not found", id, typeof(TSaga).FullName))
		{
		}
	}
}