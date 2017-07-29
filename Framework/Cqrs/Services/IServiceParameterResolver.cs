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

namespace Cqrs.Services
{
	/// <summary>
	/// Resolves parameter types when serialising with WCF.
	/// </summary>
	public interface IServiceParameterResolver
	{
		/// <summary>
		/// Indicates if the provided <paramref name="typeName"/> is resolvable.
		/// </summary>
		Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver);

		/// <summary>
		/// Returns the <see cref="Type"/> if the <paramref name="typeName"/> is resolvable.
		/// </summary>
		bool TryResolveType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace);
	}
}
