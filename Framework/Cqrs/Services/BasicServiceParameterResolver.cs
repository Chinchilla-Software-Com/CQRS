#region Copyright
// -----------------------------------------------------------------------
// <copyright company="Chinchilla Software Limited">
//     Copyright Chinchilla Software Limited. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;
using System.Xml;
using Cqrs.Authentication;

namespace Cqrs.Services
{
	/// <summary>
	/// A <see cref="DataContractResolver"/> for use via WCF
	/// </summary>
	public abstract class BasicServiceParameterResolver<TServiceParameter, TAuthenticationToken> : DataContractResolver, IServiceParameterResolver
	{
		protected BasicServiceParameterResolver(ISingleSignOnTokenResolver tokenResolver, IEventDataResolver eventDataResolver)
		{
			TokenResolver = tokenResolver;
			EventDataResolver = eventDataResolver;
			ServiceNamespace = string.Format("https://getcqrs.net/{0}", typeof(TServiceParameter).FullName);
		}

		protected BasicServiceParameterResolver(IEventDataResolver eventDataResolver)
		{
			TokenResolver = new BasicTokenResolver();
			EventDataResolver = eventDataResolver;
			ServiceNamespace = string.Format("https://getcqrs.net/{0}", typeof(TServiceParameter).FullName);
		}

		protected IServiceParameterResolver TokenResolver { get; private set; }

		protected IEventDataResolver EventDataResolver { get; private set; }

		protected string ServiceNamespace { get; private set; }

		public override bool TryResolveType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
		{
			if (dataContractType == typeof(ServiceResponse))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("ServiceResponse");
				typeNamespace = dictionary.Add(ServiceNamespace);
				return true;
			}

			if (dataContractType == typeof(ServiceRequest<TAuthenticationToken>))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("ServiceRequest");
				typeNamespace = dictionary.Add(ServiceNamespace);
				return true;
			}

			bool result = TokenResolver.TryResolveType(dataContractType, declaredType, knownTypeResolver, out typeName, out typeNamespace);
			if (result)
				return true;

			result = EventDataResolver.TryResolveType(dataContractType, declaredType, knownTypeResolver, out typeName, out typeNamespace);
			if (result)
				return true;

			result = TryResolveUnResolvedType(dataContractType, declaredType, knownTypeResolver, ref typeName, ref typeNamespace);
			if (result)
				return true;

			// Defer to the known type resolver
			return knownTypeResolver.TryResolveType(dataContractType, declaredType, null, out typeName, out typeNamespace);
		}

		protected abstract bool TryResolveUnResolvedType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, ref XmlDictionaryString typeName, ref XmlDictionaryString typeNamespace);

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
				if (typeName == "ServiceResponse")
				{
					return typeof(ServiceResponse);
				}

				if (typeName == "ServiceRequest")
				{
					return typeof(ServiceRequest<TAuthenticationToken>);
				}
			}

			Type result = TokenResolver.ResolveName(typeName, typeNamespace, declaredType, knownTypeResolver);
			if (result != null)
				return result;

			result = EventDataResolver.ResolveName(typeName, typeNamespace, declaredType, knownTypeResolver);
			if (result != null)
				return result;

			result = ResolveUnResolvedName(typeName, typeNamespace, declaredType, knownTypeResolver);
			if (result != null)
				return result;

			// Defer to the known type resolver
			return knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, null);
		}

		protected abstract Type ResolveUnResolvedName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver);
	}
}