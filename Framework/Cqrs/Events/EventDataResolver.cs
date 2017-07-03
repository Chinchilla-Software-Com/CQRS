#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using Cqrs.Services;

namespace Cqrs.Events
{
	public class EventDataResolver<TAuthenticationToken> : IEventDataResolver
	{
		#region Implementation of IServiceParameterResolver

		public virtual bool TryResolveType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
		{
			if (dataContractType == typeof(EventData))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("EventData");
				typeNamespace = dictionary.Add("https://getcqrs.net");
				return true;
			}

			if (dataContractType == typeof(ServiceRequestWithData<TAuthenticationToken, Guid>))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("EventDataGetRequest");
				typeNamespace = dictionary.Add("https://getcqrs.net");
				return true;
			}

			if (dataContractType == typeof(ServiceResponseWithResultData<IEnumerable<EventData>>))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("EventDataGetResponse");
				typeNamespace = dictionary.Add("https://getcqrs.net");
				return true;
			}

			typeName = null;
			typeNamespace = null;
			return false;
		}

		public virtual Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
		{
			if (typeName == "EventData" && typeNamespace == "https://getcqrs.net")
				return typeof(EventData);

			if (typeName == "EventDataGetRequest" && typeNamespace == "https://getcqrs.net")
				return typeof(ServiceRequestWithData<TAuthenticationToken, Guid>);

			if (typeName == "EventDataGetResponse" && typeNamespace == "https://getcqrs.net")
				return typeof(ServiceResponseWithResultData<IEnumerable<EventData>>);

			return null;
		}

		#endregion
	}
}