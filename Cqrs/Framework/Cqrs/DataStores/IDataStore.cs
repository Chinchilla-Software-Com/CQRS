using System.Collections.Generic;
using System.Linq;

namespace Cqrs.DataStores
{
	/// <summary>
	/// A data store capable of being queried and modified
	/// </summary>
	public interface IDataStore<TData> : IOrderedQueryable<TData>
	{
		void Add(TData data);

		void Add(IEnumerable<TData> data);

		void Remove(TData data);

		void RemoveAll();

		void Update(TData data);
	}
}