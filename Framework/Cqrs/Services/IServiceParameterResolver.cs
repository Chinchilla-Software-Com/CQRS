using System;
using System.Runtime.Serialization;
using System.Xml;

namespace Cqrs.Services
{
	public interface IServiceParameterResolver
	{
		Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver);

		bool TryResolveType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace);
	}
}
