#region Copyright
// -----------------------------------------------------------------------
// <copyright company="cdmdotnet Limited">
//     Copyright cdmdotnet Limited. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#endregion

using Cqrs.DataStores;
using MyCompany.MyProject.Domain.Inventory.Entities;

namespace MyCompany.MyProject.Domain.Factories
{
	/// <summary>
	/// A factory for obtaining <see cref="IDataStore{TData}"/> instances using an InProcess only approach
	/// </summary>
	public partial class DomainInProcessDataStoreFactory
	{
		#region Implementation of IDomainDataStoreFactory

		public IDataStore<InventoryItemSummaryEntity> GetInventoryItemSummaryDataStore()
		{
			IDataStore<InventoryItemSummaryEntity> result = new InProcessDataStore<InventoryItemSummaryEntity>();
			return result;
		}

		#endregion
	}
}
