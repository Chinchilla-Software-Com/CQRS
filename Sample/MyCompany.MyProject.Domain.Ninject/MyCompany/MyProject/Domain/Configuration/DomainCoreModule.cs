#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using MyCompany.MyProject.Domain.Inventory.Repositories;

namespace MyCompany.MyProject.Domain.Configuration
{
	public class DomainCoreModule : DomainNinjectModule
	{
		#region Overrides of DomainNinjectModule

		/// <summary>
		/// Register the all repositories
		/// </summary>
		public override void RegisterRepositories()
		{
			base.RegisterRepositories();

			Bind<IInventoryItemSummaryRepository>()
				.To<InventoryItemSummaryRepository>()
				.InSingletonScope();
		}

		#endregion
	}
}