#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;
using System.Xml;
using Cqrs.Services;

namespace Cqrs.Authentication
{
	/// <summary>
	/// Resolves parameter types when serialising with WCF of <see cref="Type"/>
	/// <see cref="SingleSignOnToken"/>, <see cref="SingleSignOnTokenWithUserRsn"/>, <see cref="SingleSignOnTokenWithCompanyRsn"/> and <see cref="SingleSignOnTokenWithUserRsnAndCompanyRsn"/>
	/// </summary>
	public class SingleSignOnTokenResolver : ISingleSignOnTokenResolver
	{
		#region Implementation of IServiceParameterResolver

		/// <summary>
		/// Indicates if the provided <paramref name="dataContractType"/> is of type <see cref="SingleSignOnToken"/>, <see cref="SingleSignOnTokenWithUserRsn"/>, <see cref="SingleSignOnTokenWithCompanyRsn"/>, <see cref="SingleSignOnTokenWithUserRsnAndCompanyRsn"/>
		/// OR if it is other resolvable.
		/// </summary>
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

		/// <summary>
		/// Returns the <see cref="Type"/> if the <paramref name="typeName"/> is resolvable or if it is 
		/// of type <see cref="SingleSignOnToken"/>, <see cref="SingleSignOnTokenWithUserRsn"/>, <see cref="SingleSignOnTokenWithCompanyRsn"/> and <see cref="SingleSignOnTokenWithUserRsnAndCompanyRsn"/>
		/// </summary>
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