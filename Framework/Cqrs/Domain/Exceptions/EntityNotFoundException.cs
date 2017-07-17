#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Entities;

namespace Cqrs.Domain.Exceptions
{
	/// <summary>
	/// The <see cref="IEntity"/> requested was not found.
	/// </summary>
	/// <typeparam name="TEntity">The <see cref="Type"/> of the <see cref="IEntity"/> that wasn't found</typeparam>
	[Serializable]
	public class EntityNotFoundException<TEntity> : Exception
		where TEntity : IEntity
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="EntityNotFoundException{TEntity}"/> with the provided identifier of the <see cref="IEntity"/> that wasn't found.
		/// </summary>
		/// <param name="id">The identifier of the <see cref="IEntity"/> that wasn't found.</param>
		public EntityNotFoundException(Guid id)
			: base(string.Format("Entity '{0}' of type '{1}' was not found", id, typeof(TEntity).FullName))
		{
		}
	}
}