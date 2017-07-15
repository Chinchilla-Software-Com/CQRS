namespace Chat.MicroServices.Configuration
{
	using Authentication.Configuration;
	using Authentication.Helpers;
	using Conversations.Configuration;
	using Factories;
	using Ninject.Modules;

	public class QueriesModule : NinjectModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			LoadFactories();
			LoadRepositories();
			LoadHelpers();
		}

		#endregion

		/// <summary>
		/// Load all required Repositories
		/// </summary>
		protected virtual void LoadRepositories()
		{
			new AuthenticationModule().LoadRepositories(Kernel);
			new ConversationsModule().LoadRepositories(Kernel);
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

		/// <summary>
		/// Load all required helpers.
		/// </summary>
		protected virtual void LoadHelpers()
		{
			Bind<IAuthenticationHashHelper>()
				.To<AuthenticationHashHelper>()
				.InSingletonScope();
		}
	}
}