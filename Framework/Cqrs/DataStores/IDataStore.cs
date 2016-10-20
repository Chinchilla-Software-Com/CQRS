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

namespace Cqrs.DataStores
{
	/// <summary>
	/// A data store capable of being queried and modified
	/// </summary>
	public interface IDataStore<TData> : IOrderedQueryable<TData>, IDisposable
	{
		void Add(TData data);

		void Add(IEnumerable<TData> data);

		/// <summary>
		/// Will mark the <paramref name="data"/> as logically (or soft).
		/// </summary>
		void Remove(TData data);

		void Destroy(TData data);

		void RemoveAll();

		void Update(TData data);
	}
}