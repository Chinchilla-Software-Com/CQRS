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

namespace Cqrs.Authentication
{
	public class SingleSignOnTokenResolver : ISingleSignOnTokenResolver
	{
		#region Implementation of IServiceParameterResolver

		public virtual bool TryResolveType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
		{
			if (dataContractType == typeof(SingleSignOnToken))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("SingleSignOnToken");
				typeNamespace = dictionary.Add("http://tempuri.com");
				return true;
			}

			typeName = null;
			typeNamespace = null;
			return false;
		}

		public virtual Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
		{
			if (typeName == "SingleSignOnToken" && typeNamespace == "http://tempuri.com")
				return typeof(SingleSignOnToken);

			return null;
		}

		#endregion
	}
}