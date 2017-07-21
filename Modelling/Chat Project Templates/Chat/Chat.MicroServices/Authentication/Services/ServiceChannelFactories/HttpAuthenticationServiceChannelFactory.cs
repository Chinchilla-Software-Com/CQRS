namespace $safeprojectname$.Authentication.Services.ServiceChannelFactories
{
	using Cqrs.Services;
	using WcfResolvers;

	/// <summary>
	/// A <see cref="ServiceChannelFactory{TService}"/> for using  <see cref="IAuthenticationService"/> via WCF
	/// </summary>
	public class HttpAuthenticationServiceChannelFactory : ServiceChannelFactory<IAuthenticationService>
	{
		/// <summary>
		/// Instantiates a new instance of the <see cref="HttpAuthenticationServiceChannelFactory"/> class with the default endpoint configuration name of HttpAuthenticationService.
		/// </summary>
		public HttpAuthenticationServiceChannelFactory()
			: this("HttpAuthenticationService")
		{
		}

		/// <summary>
		/// Instantiates a new instance of the <see cref="HttpAuthenticationServiceChannelFactory" /> class with a specified endpoint configuration name.
		/// </summary>
		/// <param name="endpointConfigurationName">The configuration name used for the endpoint.</param>
		public HttpAuthenticationServiceChannelFactory(string endpointConfigurationName)
			: base(endpointConfigurationName)
		{
		}

		protected override void RegisterDataContracts()
		{
			AuthenticationServiceLoginParametersResolver.RegisterDataContracts();
		}
	}
}