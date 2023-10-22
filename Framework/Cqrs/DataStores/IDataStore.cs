#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cqrs.Entities;

namespace Cqrs.DataStores
{
	/// <summary>
	/// A data store capable of being queried and modified.
	/// </summary>
	public interface IDataStore<TData> : IOrderedQueryable<TData>, IDisposable
	{
		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>

#if NET40
		void Add
#else
		Task AddAsync
#endif
			(TData data);

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
#if NET40
		void Add
#else
		Task AddAsync
#endif
			(IEnumerable<TData> data);

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft) deleted by setting <see cref="Entity.IsDeleted"/> to true in the data store and persist the change.
		/// </summary>
#if NET40
		void Remove
#else
		Task RemoveAsync
#endif
			(TData data);

		/// <summary>
		/// Remove the provided <paramref name="data"/> (normally by <see cref="IEntity.Rsn"/>) from the data store and persist the change.
		/// </summary>
#if NET40
		void Destroy
#else
		Task DestroyAsync
#endif
			(TData data);

		/// <summary>
		/// Remove all contents (normally by use of a truncate operation) from the data store and persist the change.
		/// </summary>
#if NET40
		void RemoveAll
#else
		Task RemoveAllAsync
#endif
			();

		/// <summary>
		/// Update the provided <paramref name="data"/> in the data store and persist the change.
		/// </summary>
#if NET40
		void Update
#else
		Task UpdateAsync
#endif
			(TData data);
	}
}