#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Entities;

namespace Cqrs.DataStores
{
	/// <summary>
	/// A data store capable of being queried and modified
	/// </summary>
	public interface IDataStore<TData> : IOrderedQueryable<TData>, IDisposable
	{
		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		void Add(TData data);

		/// <summary>
		/// Add the provided <paramref name="data"/> to the data store and persist the change.
		/// </summary>
		void Add(IEnumerable<TData> data);

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft) deleted by setting <see cref="Entity.IsLogicallyDeleted"/> to true in the data store and persist the change.
		/// </summary>
		void Remove(TData data);

		/// <summary>
		/// Remove the provided <paramref name="data"/> (normally by <see cref="IEntity.Rsn"/>) from the data store and persist the change.
		/// </summary>
		void Destroy(TData data);

		/// <summary>
		/// Remove all contents (normally by use of a truncate operation) from the data store and persist the change.
		/// </summary>
		void RemoveAll();

		/// <summary>
		/// Update the provided <paramref name="data"/> in the data store and persist the change.
		/// </summary>
		void Update(TData data);
	}
}