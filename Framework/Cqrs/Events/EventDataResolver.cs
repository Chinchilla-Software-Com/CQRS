#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;
using System.Xml;
using Cqrs.Services;

namespace Cqrs.Events
{
	public class EventDataResolver : IEventDataResolver
	{
		#region Implementation of IServiceParameterResolver

		public virtual bool TryResolveType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
		{
			if (dataContractType == typeof(EventData))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("EventData");
				typeNamespace = dictionary.Add("http://cqrs.co.nz");
				return true;
			}

			typeName = null;
			typeNamespace = null;
			return false;
		}

		public virtual Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
		{
			if (typeName == "EventData" && typeNamespace == "http://cqrs.co.nz")
				return typeof(EventData);

			return null;
		}

		#endregion
	}
}