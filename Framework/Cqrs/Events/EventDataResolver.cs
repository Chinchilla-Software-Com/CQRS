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
	/// <summary>
	/// Resolves <see cref="EventData"/>, <see cref="ServiceRequestWithData{TAuthenticationToken, Guid}" /> and <see cref="ServiceResponseWithResultData{IEnumerableEventData}"/> parameter types when serialising with WCF.
	/// </summary>
	public class EventDataResolver<TAuthenticationToken> : IEventDataResolver
	{
		#region Implementation of IServiceParameterResolver

		/// <summary>
		/// Indicates if the provided <paramref name="dataContractType"/> is of type <see cref="EventData"/>, <see cref="ServiceRequestWithData{TAuthenticationToken, Guid}" />, <see cref="ServiceResponseWithResultData{IEnumerableEventData}"/>0
		/// OR if it is other resolvable.
		/// </summary>
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

		/// <summary>
		/// Returns the <see cref="Type"/> if the <paramref name="typeName"/> is resolvable or if it is 
		/// of type <paramref name="typeName"/> is of type <see cref="EventData"/>, <see cref="ServiceRequestWithData{TAuthenticationToken, Guid}" />, <see cref="ServiceResponseWithResultData{IEnumerableEventData}"/>
		/// </summary>
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