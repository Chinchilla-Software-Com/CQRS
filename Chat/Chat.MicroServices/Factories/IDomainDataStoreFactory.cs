
using Chat.MicroServices.Conversations.Entities;

namespace Chat.MicroServices.Factories
{
	using Authentication.Entities;
	using Cqrs.DataStores;

	/// <summary>
	/// A factory for obtaining <see cref="IDataStore{TData}"/> instances
	/// </summary>
	public interface IDomainDataStoreFactory
	{
		/// <summary>
		/// Get a <see cref="IDataStore{TData}"/> to access <see cref="CredentialEntity"/>
		/// </summary>
		IDataStore<CredentialEntity> GetCredentialDataStore();

		/// <summary>
		/// Get a <see cref="IDataStore{TData}"/> to access <see cref="UserEntity"/>
		/// </summary>
		IDataStore<UserEntity> GetUserDataStore();

		/// <summary>
		/// Get a <see cref="IDataStore{TData}"/> to access <see cref="ConversationSummaryEntity"/>
		/// </summary>
		IDataStore<ConversationSummaryEntity> GetConversationSummaryDataStore();

		/// <summary>
		/// Get a <see cref="IDataStore{TData}"/> to access <see cref="MessageEntity"/>
		/// </summary>
		IDataStore<MessageEntity> GetMessageDataStore();
	}
}