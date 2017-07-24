#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Azure.BlobStorage.DataStores;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.BlobStorage.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="TableStorageDataStoreConnectionStringFactory"/> as the <see cref="ITableStorageDataStoreConnectionStringFactory"/>.
	/// </summary>
	public class TableStorageDataStoreModule : NinjectModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			RegisterFactories();
		}

		#endregion

		/// <summary>
		/// Register the all factories
		/// </summary>
		public virtual void RegisterFactories()
		{
			Bind<ITableStorageDataStoreConnectionStringFactory>()
				.To<TableStorageDataStoreConnectionStringFactory>()
				.InSingletonScope();
		}
	}
}