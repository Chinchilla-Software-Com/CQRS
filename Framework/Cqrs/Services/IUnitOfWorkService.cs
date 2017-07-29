#region Copyright
// -----------------------------------------------------------------------
// <copyright company="Chinchilla Software Limited">
//     Copyright Chinchilla Software Limited. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#endregion

using Cqrs.Domain;

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
		bool Commit(object commiter);
	}
}