namespace Chat.MicroServices.Conversations.Services.WcfResolvers
{
	using Cqrs.Events;
	using Cqrs.Services;
	using System;
	using System.Runtime.Serialization;
	using System.Xml;

	/// <summary>
	/// A <see cref="DataContractResolver"/> for using <see cref="IConversationService.UpdateConversation"/> via WCF
	/// </summary>
	public class ConversationServiceUpdateConversationParametersResolver : BasicServiceParameterResolver<IConversationService, Guid>
	{
		public ConversationServiceUpdateConversationParametersResolver()
			: base(new EventDataResolver<Guid>())
		{
		}

		public override bool TryResolveType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
		{
			if (dataContractType == typeof(ServiceRequestWithData<Guid, ConversationService.UpdateConversationParameters>))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("ConversationServiceUpdateConversationParameters");
				typeNamespace = dictionary.Add(ServiceNamespace);
				return true;
			}

			return base.TryResolveType(dataContractType, declaredType, knownTypeResolver, out typeName, out typeNamespace);
		}

		protected override bool TryResolveUnResolvedType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, ref XmlDictionaryString typeName, ref XmlDictionaryString typeNamespace)
		{
			return false;
		}

		/// <summary>
		/// Override this method to map the specified xsi:type name and namespace to a data contract type during deserialization.
		/// </summary>
		/// <returns>
		/// The type the xsi:type name and namespace is mapped to. 
		/// </returns>
		/// <param name="typeName">The xsi:type name to map.</param><param name="typeNamespace">The xsi:type namespace to map.</param><param name="declaredType">The type declared in the data contract.</param><param name="knownTypeResolver">The known type resolver.</param>
		public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
		{
			if (typeNamespace == ServiceNamespace)
			{
				if (typeName == "ConversationServiceUpdateConversationParameters")
					return typeof(ServiceRequestWithData<Guid, ConversationService.UpdateConversationParameters>);
			}

			return base.ResolveName(typeName, typeNamespace, declaredType, knownTypeResolver);
		}

		protected override Type ResolveUnResolvedName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
		{
			return null;
		}

		public static void RegisterDataContracts()
		{
			WcfDataContractResolverConfiguration.Current.RegisterDataContract<IConversationService, ConversationServiceUpdateConversationParametersResolver>("UpdateConversationParameters");
		}
	}
}