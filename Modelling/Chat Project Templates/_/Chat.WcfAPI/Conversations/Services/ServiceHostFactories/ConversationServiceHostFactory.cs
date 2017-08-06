namespace $safeprojectname$.Conversations.Services.ServiceHostFactories
{
	using Cqrs.Ninject.ServiceHost;
	using MicroServices.Conversations.Services;

	/// <summary>
	/// A <see cref="NinjectWcfServiceHostFactory{TServiceType}"/> for using  <see cref="IConversationService"/> via WCF
	/// </summary>
	public class ConversationServiceHostFactory : NinjectWcfServiceHostFactory<IConversationService>
	{
	}
}