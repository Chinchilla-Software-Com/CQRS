namespace $safeprojectname$.Code
{
	using Helpers;
	using Ninject.Modules;

	public class ApiModule : NinjectModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			LoadHelpers();
		}

		#endregion

		/// <summary>
		/// Load all required helpers.
		/// </summary>
		protected virtual void LoadHelpers()
		{
			Bind<IAuthenticationHelper>()
				.To<AuthenticationHelper>()
				.InSingletonScope();
		}
	}
}