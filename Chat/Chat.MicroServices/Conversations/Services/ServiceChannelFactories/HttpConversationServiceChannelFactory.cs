namespace Chat.MicroServices.Conversations.Services.ServiceChannelFactories
{
	using Cqrs.Services;
	using WcfResolvers;

	/// <summary>
	/// A <see cref="ServiceChannelFactory{TService}"/> for using  <see cref="IConversationService"/> via WCF
	/// </summary>
	public class HttpConversationServiceChannelFactory : ServiceChannelFactory<IConversationService>
	{
		/// <summary>
		/// Instantiates a new instance of the <see cref="HttpConversationServiceChannelFactory"/> class with the default endpoint configuration name of HttpConversationService.
		/// </summary>
		public HttpConversationServiceChannelFactory()
			: this("HttpConversationService")
		{
		}

		/// <summary>
		/// Instantiates a new instance of the <see cref="HttpConversationServiceChannelFactory" /> class with a specified endpoint configuration name.
		/// </summary>
		/// <param name="endpointConfigurationName">The configuration name used for the endpoint.</param>
		public HttpConversationServiceChannelFactory(string endpointConfigurationName)
			: base(endpointConfigurationName)
		{
		}

		protected override void RegisterDataContracts()
		{
			ConversationServiceConversationParametersResolver.RegisterDataContracts();
			ConversationServicePostCommentParametersResolver.RegisterDataContracts();
			ConversationServiceStartConversationParametersResolver.RegisterDataContracts();
			ConversationServiceUpdateConversationParametersResolver.RegisterDataContracts();
		}
	}
}
