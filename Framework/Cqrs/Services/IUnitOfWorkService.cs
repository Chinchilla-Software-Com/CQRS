#region Copyright
// -----------------------------------------------------------------------
// <copyright company="Chinchilla Software Limited">
//     Copyright Chinchilla Software Limited. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#endregion

using Cqrs.Domain;
using System.Threading.Tasks;

namespace Cqrs.Services
{
	/// <summary>
	/// Provides a basic container to control how the <see cref="IUnitOfWork{TAuthenticationToken}"/> is accessed.
	/// </summary>
	public interface IUnitOfWorkService
	{
		/// <summary>
		/// Informs the service of the object that will be committing the UnitOfWork.
		/// </summary>
		/// <returns>
		/// true if the provided <paramref name="commiter"/> is accepted as the committer, false otherwise.
		/// </returns>
		bool SetCommitter(object commiter);

		/// <summary>
		/// Commits the UnitOfWork if the provided <paramref name="commiter"/> is the Committer.
		/// </summary>
		/// <returns>
		/// true if the provided <paramref name="commiter"/> is the Committer, false otherwise.
		/// </returns>
#if NET40
		bool Commit
#else
		Task<bool> CommitAsync
#endif
			(object commiter);
	}
}