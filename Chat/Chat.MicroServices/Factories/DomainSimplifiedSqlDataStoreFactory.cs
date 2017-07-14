using Chat.MicroServices.Conversations.Entities;

namespace Chat.MicroServices.Factories
{
	using Authentication.Entities;
	using cdmdotnet.Logging;
	using Cqrs.Configuration;
	using Cqrs.DataStores;

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

		/// <summary>
		/// Get a <see cref="IDataStore{TData}"/> to access <see cref="CredentialEntity"/>
		/// </summary>
		public virtual IDataStore<CredentialEntity> GetCredentialDataStore()
		{
			IDataStore<CredentialEntity> result = new SqlDataStore<CredentialEntity>(ConfigurationManager, Logger);
			return result;
		}

		/// <summary>
		/// Get a <see cref="IDataStore{TData}"/> to access <see cref="UserEntity"/>
		/// </summary>
		public IDataStore<UserEntity> GetUserDataStore()
		{
			IDataStore<UserEntity> result = new SqlDataStore<UserEntity>(ConfigurationManager, Logger);
			return result;
		}

		/// <summary>
		/// Get a <see cref="IDataStore{TData}"/> to access <see cref="ConversationSummaryEntity"/>
		/// </summary>
		public IDataStore<ConversationSummaryEntity> GetConversationSummaryDataStore()
		{
			IDataStore<ConversationSummaryEntity> result = new SqlDataStore<ConversationSummaryEntity>(ConfigurationManager, Logger);
			return result;
		}

		/// <summary>
		/// Get a <see cref="IDataStore{TData}"/> to access <see cref="MessageEntity"/>
		/// </summary>
		public IDataStore<MessageEntity> GetMessageDataStore()
		{
			IDataStore<MessageEntity> result = new SqlDataStore<MessageEntity>(ConfigurationManager, Logger);
			return result;
		}

		#endregion
	}
}