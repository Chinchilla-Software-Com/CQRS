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
	public class UnitOfWorkService<TAuthenticationToken> : IUnitOfWorkService
	{
		protected IUnitOfWork<TAuthenticationToken> UnitOfWork { get; private set; }

		protected object Committer { get; private set; }

		public UnitOfWorkService(IUnitOfWork<TAuthenticationToken> unitOfWork)
		{
			UnitOfWork = unitOfWork;
		}

		/// <summary>
		/// Informs the service of the object that will be committing the <see cref="UnitOfWork"/>.
		/// </summary>
		/// <returns>
		/// true if the provided <paramref name="commiter"/> is accepted as the committer, false otherwise.
		/// </returns>
		public bool SetCommitter(object commiter)
		{
			if (Committer != null)
				return false;

			Committer = commiter;
			return true;
		}

		/// <summary>
		/// Commits the <see cref="UnitOfWork"/> if the provided <paramref name="commiter"/> is the <see cref="Committer"/>.
		/// </summary>
		/// <returns>
		/// true if the provided <paramref name="commiter"/> is the <see cref="Committer"/>, false otherwise.
		/// </returns>
		public bool Commit(object commiter)
		{
			if (Committer != commiter)
				return false;

			UnitOfWork.Commit();
			return true;
		}
	}
}