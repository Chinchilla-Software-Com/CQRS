namespace $safeprojectname$.Authentication.Configuration
{
	using Ninject;
	using Repositories;

	public class AuthenticationModule
	{
		/// <summary>
		/// Load all required Repositories
		/// </summary>
		public virtual void LoadRepositories(IKernel kernel)
		{
			kernel.Bind<ICredentialRepository>()
				.To<CredentialRepository>()
				.InSingletonScope();
			kernel.Bind<IUserRepository>()
				.To<UserRepository>()
				.InSingletonScope();
		}
	}
}