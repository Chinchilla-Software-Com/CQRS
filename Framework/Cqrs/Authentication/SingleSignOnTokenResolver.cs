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
			if (dataContractType == typeof(SingleSignOnTokenWithUserRsnAndCompanyRsn))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("SingleSignOnTokenWithUserAndCompanyRsn");
				typeNamespace = dictionary.Add("https://getcqrs.net");
				return true;
			}

			if (dataContractType == typeof(SingleSignOnTokenWithUserRsn))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("SingleSignOnTokenWithUserRsn");
				typeNamespace = dictionary.Add("https://getcqrs.net");
				return true;
			}

			if (dataContractType == typeof(SingleSignOnTokenWithCompanyRsn))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("SingleSignOnTokenWithCompanyRsn");
				typeNamespace = dictionary.Add("https://getcqrs.net");
				return true;
			}

			if (dataContractType == typeof(SingleSignOnToken))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("SingleSignOnToken");
				typeNamespace = dictionary.Add("https://getcqrs.net");
				return true;
			}

			typeName = null;
			typeNamespace = null;
			return false;
		}

		public virtual Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
		{
			switch (typeNamespace)
			{
				case "https://getcqrs.net":
					switch (typeName)
					{
						case "SingleSignOnToken":
							return typeof(SingleSignOnToken);
						case "SingleSignOnTokenWithCompanyRsn":
							return typeof(SingleSignOnTokenWithCompanyRsn);
						case "SingleSignOnTokenWithUserRsn":
							return typeof(SingleSignOnTokenWithUserRsn);
						case "SingleSignOnTokenWithUserAndCompanyRsn":
							return typeof(SingleSignOnTokenWithUserRsnAndCompanyRsn);
					}
					break;
			}

			return null;
		}

		#endregion
	}
}