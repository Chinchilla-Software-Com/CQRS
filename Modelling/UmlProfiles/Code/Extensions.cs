using System;
using Microsoft.VisualStudio.Uml.Classes;

namespace Cqrs.Modelling.UmlProfiles
{
	public static class CSharpExtensions
	{
		[Obsolete("Not Implemented")]
		public static string FindImpliedStereotype(this IElement element, string a)
		{
			throw new NotImplementedException();
		}

		[Obsolete("Not Implemented")]
		public static string GetStereotypeProperty(this IElement element, string ProfileName, string stereotypeName, string property)
		{
			throw new NotImplementedException();
		}
	}

	public static class CSharpConstants
	{
		public const string ProfileName = "Profile";
		public const string NamespaceStereotypeName = "NamespaceStereotype";
		public const string UsingNamespacesStereotypePropertyName = "UsingNamespacesStereotypeProperty";
	}
}