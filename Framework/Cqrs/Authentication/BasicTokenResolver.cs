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
	/// Resolves basic, known parameter types when serialising with WCF.
	/// </summary>
	public class BasicTokenResolver : IServiceParameterResolver
	{
		#region Implementation of IServiceParameterResolver

		/// <summary>
		/// Indicates if the provided <paramref name="dataContractType"/> is of type <see cref="Guid"/>, <see cref="Nullable{Guid}"/>, <see cref="int"/>, <see cref="Nullable{integer}"/>, <see cref="string"/>
		/// OR if it is other resolvable.
		/// </summary>
		public virtual bool TryResolveType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
		{
			if (dataContractType == typeof(Guid))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("GuidToken");
				typeNamespace = dictionary.Add("https://getcqrs.net");
				return true;
			}

			if (dataContractType == typeof(Guid?))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("NullableGuidToken");
				typeNamespace = dictionary.Add("https://getcqrs.net");
				return true;
			}

			if (dataContractType == typeof(string))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("StringToken");
				typeNamespace = dictionary.Add("https://getcqrs.net");
				return true;
			}

			if (dataContractType == typeof(int))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("IntegerToken");
				typeNamespace = dictionary.Add("https://getcqrs.net");
				return true;
			}

			if (dataContractType == typeof(int?))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add("NullableIntegerToken");
				typeNamespace = dictionary.Add("https://getcqrs.net");
				return true;
			}

			typeName = null;
			typeNamespace = null;
			return false;
		}

		/// <summary>
		/// Returns the <see cref="Type"/> if the <paramref name="typeName"/> is resolvable or if it is 
		/// of type <see cref="Guid"/>, <see cref="Nullable{Guid}"/>, <see cref="int"/>, <see cref="Nullable{integer}"/> and <see cref="string"/>
		/// </summary>
		public virtual Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
		{
			switch (typeNamespace)
			{
				case "https://getcqrs.net":
					switch (typeName)
					{
						case "GuidToken":
							return typeof(Guid);
						case "NullableGuidToken":
							return typeof(Guid?);
						case "StringToken":
							return typeof(string);
						case "IntegerToken":
							return typeof(int);
						case "NullableIntegerToken":
							return typeof(int?);
					}
					break;
			}

			return null;
		}

		#endregion
	}
}