namespace KendoUI.Northwind.Dashboard
{
	using Code.Factories;
	using Code.Repositories;
	using Ninject.Modules;

	public class NorthwindModule : NinjectModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			LoadFactories();
			LoadRepositories();
		}

		#endregion

		/// <summary>
		/// Load all required Repositories
		/// </summary>
		protected virtual void LoadRepositories()
		{
			Bind<IOrderRepository>()
				.To<OrderRepository>()
				.InSingletonScope();
		}

		/// <summary>
		/// Load all required factories.
		/// </summary>
		protected virtual void LoadFactories()
		{
			Bind<IDomainDataStoreFactory>()
				.To<DomainSimplifiedSqlDataStoreFactory>()
				.InSingletonScope();
		}
	}
}