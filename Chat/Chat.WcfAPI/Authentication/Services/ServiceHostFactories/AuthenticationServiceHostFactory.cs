namespace Chat.WcfAPI.Authentication.Services.ServiceHostFactories
{
	using Cqrs.Ninject.ServiceHost;
	using MicroServices.Authentication.Services;

	/// <summary>
	/// A <see cref="NinjectWcfServiceHostFactory{TServiceType}"/> for using  <see cref="IAuthenticationService"/> via WCF
	/// </summary>
	public class AuthenticationServiceHostFactory : NinjectWcfServiceHostFactory<IAuthenticationService>
	{
	}
}