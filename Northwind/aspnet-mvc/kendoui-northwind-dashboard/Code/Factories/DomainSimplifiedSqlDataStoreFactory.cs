namespace KendoUI.Northwind.Dashboard.Code.Factories
{
	using cdmdotnet.Logging;
	using Cqrs.Configuration;
	using Cqrs.DataStores;
	using Entities;

	/// <summary>
	/// A factory for obtaining <see cref="IDataStore{TData}"/> instances using the built-in simplified Sql
	/// </summary>
	public class DomainSimplifiedSqlDataStoreFactory : IDomainDataStoreFactory
	{
		public DomainSimplifiedSqlDataStoreFactory(ILogger logger, IConfigurationManager configurationManager)
		{
			Logger = logger;
			ConfigurationManager = configurationManager;
		}

		protected ILogger Logger { get; private set; }

		protected IConfigurationManager ConfigurationManager { get; private set; }

		#region Implementation of IDomainDataStoreFactory

		public virtual IDataStore<OrderEntity> GetOrderDataStore()
		{
			IDataStore<OrderEntity> result = new SqlDataStore<OrderEntity>(ConfigurationManager, Logger);
			return result;
		}

		#endregion
	}
}