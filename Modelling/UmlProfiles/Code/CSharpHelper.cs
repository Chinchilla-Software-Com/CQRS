using Microsoft.VisualStudio.ArchitectureTools.Extensibility.Uml;
using Microsoft.VisualStudio.Uml.AuxiliaryConstructs;
using Microsoft.VisualStudio.Uml.Classes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cqrs.Modelling.UmlProfiles
{
	public class CSharpHelper
	{
		public static Action<string> Write { get; set; }

		public static Action<string> WriteLine { get; set; }

		public static Action PopIndent { get; set; }
		public static Action<string> PushIndent { get; set; }

		#region CSharpStereotypeHelpers.t4
		// CSharp profile name
		internal static readonly string ProfileName = "CSharpProfile";

		/// <summary>
		/// Get the visibility of the supplied element
		/// </summary>
		/// <param name="namedElement">The element in question</param>
		/// <param name="stereotypeName">The stereotype name</param>
		/// <param name="stereotypePropertyName">The stereotype property name</param>
		/// <returns>public, protected, protected internal, internal, or private</returns>
		internal static string Visibility(INamedElement namedElement, string stereotypeName, string stereotypePropertyName)
		{
			switch (namedElement.Visibility)
			{
				case VisibilityKind.Public: return "public ";
				case VisibilityKind.Protected: return "protected ";
				case VisibilityKind.Private: return "private ";
				case VisibilityKind.Package:
					{
						string getVisibility = GetProperty(namedElement, stereotypeName, stereotypePropertyName);
						if (getVisibility == "protectedinternal")
						{
							return "protected internal ";
						}

						return "internal ";
					}

				default: return string.Empty;
			}
		}

		/// <summary>
		/// Get the visibility of the given method. Note that if the method is not partial
		/// you can't have any access modifier.
		/// </summary>
		/// <param name="method">The method in question</param>
		/// <returns>public, protected, protected internal, internal, or private</returns>
		internal static string MethodVisibility(IOperation method)
		{
			if (string.IsNullOrEmpty(MethodPartialOption(method)))
			{
				return Visibility(method, "method", "PackageVisibility");
			}
			else
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// Get the visibility of the given indexer.
		/// </summary>
		/// <param name="indexer">The indexer in question</param>
		/// <returns>public, protected, protected internal, internal, or private</returns>
		internal static string IndexerVisibility(IOperation indexer)
		{
			return Visibility(indexer, "indexer", "PackageVisibility");
		}

		/// <summary>
		/// Returns the visibility of the given field.
		/// </summary>
		/// <param name="field">field</param>
		/// <returns>public, protected, protected internal, internal, or private</returns>
		internal static string FieldVisibility(IProperty field)
		{
			return Visibility(field, "field", "PackageVisibility");
		}

		/// <summary>
		/// Returns the visibility of the given property.
		/// </summary>
		/// <param name="property">property</param>
		/// <returns>public, protected, protected internal, internal, or private</returns>
		internal static string PropertyVisibility(IProperty property)
		{
			return Visibility(property, "property", "PackageVisibility");
		}

		/// <summary>
		/// Returns the visibility of the given property getter
		/// </summary>
		/// <param name="property">The property</param>
		/// <returns>The property getter visibility</returns>
		internal static string PropertyGetVisibility(IProperty property)
		{
			string getVisibility = GetProperty(property, "property", "Get");
			return GetterSetterVisibility(getVisibility);
		}

		/// <summary>
		/// Returns the visibility of the given property setter
		/// </summary>
		/// <param name="element">element</param>
		/// <returns>string visibility</returns>
		internal static string PropertySetVisibility(IProperty element)
		{
			string getVisibility = GetProperty(element, "property", "Set");
			return GetterSetterVisibility(getVisibility);
		}

		/// <summary>
		/// Returns the visibility of the given indexer's getter
		/// </summary>
		/// <param name="operation">The operation</param>
		/// <returns>The indexer getter visibility</returns>
		internal static string IndexerGetVisibility(IOperation operation)
		{
			string getVisibility = GetProperty(operation, "indexer", "Get");
			return GetterSetterVisibility(getVisibility);
		}

		/// <summary>
		/// Returns the visibility of the given indexer's setter
		/// </summary>
		/// <param name="element">element</param>
		/// <returns>The indexer setter visibility</returns>
		internal static string IndexerSetVisibility(IOperation element)
		{
			string getVisibility = GetProperty(element, "indexer", "Set");
			return GetterSetterVisibility(getVisibility);
		}

		///<summary>
		/// Returns the generated visibility for the visibility selected by the user
		///</summary>
		///<param name="selectedVisibility">The visibility selected by user for getter or setter</param>
		/// <returns>The getter/setter visibility</returns>
		internal static string GetterSetterVisibility(string selectedVisibility)
		{
			//the access modifier keyword 'public' never needs to be generated for the getter or setter, at most it would be redundant
			string getVisibility = selectedVisibility;
			if (getVisibility == "none" || getVisibility == "public")
			{
				getVisibility = string.Empty;
			}
			else if (getVisibility == "package")
			{
				getVisibility = "internal";
			}
			else if (getVisibility == "protectedinternal")
			{
				getVisibility = "protected internal";
			}

			return string.IsNullOrWhiteSpace(getVisibility) ? string.Empty : getVisibility + " ";
		}

		///<summary>
		/// Returns the keyword of certain stereotype property for the element.
		///</summary>
		/// <param name="element">The element</param>
		/// <param name="stereotypeName">The stereotype name</param>
		/// <param name="propertyName">The stereotype property name</param>
		/// <param name="keyword">The keyword corresponding to the property name</param>
		/// <returns>The property keyword</returns>
		internal static string GetProperty(IElement element, string stereotypeName, string propertyName, string keyword)
		{
			string property = GetProperty(element, stereotypeName, propertyName);
			if (!string.IsNullOrEmpty(property) && Convert.ToBoolean(property))
			{
				return keyword + " ";
			}

			return string.Empty;
		}

		/// <summary>
		/// Returns the value of a stereotype property
		/// </summary>
		/// <param name="property">stereotype property name</param>
		/// <returns>string value</returns>
		internal static string GetProperty(IElement element, string stereotypeName, string property)
		{
			return element.GetStereotypeProperty(ProfileName, stereotypeName, property) ?? string.Empty;
		}

		/// <summary>
		/// Get the default stereotype for the encapsulated UML model element if there is
		/// no stereotype set explicitly;
		/// Get the stereotype that is set if there is only one CSharp stereotype set;
		/// Return null, if there are multiple stereotypes set for the element.
		/// </summary>
		/// <param name="element">the element in question</param>
		/// <returns>The name of the element's stereotype it should apply</returns>
		internal static string GetStereotype(IElement element)
		{
			var csharpStereotypes = element.AppliedStereotypes.Where(s => s.Profile == ProfileName);

			if (!csharpStereotypes.Any())
			{
				return GetDefaultStereotype(element);
			}
			else if (csharpStereotypes.Count() == 1)
			{
				return csharpStereotypes.First() != null ? csharpStereotypes.First().Name : null;
			}

			return null;
		}

		/// <summary>
		/// Get the default stereotype for the encapsulated UML model element (i.e. The name 
		/// of the stereotype implied if the element has no other stereotypes applied).
		/// </summary>
		/// <param name="element">the element in question</param>
		/// <remarks>
		/// The mapping From a type to its default stereotype:
		/// --------------------------------------------------            
		/// IClass          "class"         // IClass never implies "struct" or "delegate"
		/// IEnumeration    "enum"
		/// IDependency     "extends"
		/// IProperty       "property"
		/// IInterface      "interface"
		/// IOperation      "method"        // IOperation never implies "indexer"
		/// IPackage        "namespace"
		/// IPackageImport  "using"
		/// </remarks> 
		/// <returns>The name of the element's default stereotype.  </returns>
		internal static string GetDefaultStereotype(IElement element)
		{
			return element.FindImpliedStereotype("CSharpProfile");
		}

		private void WriteFieldClrAttributes(IProperty element)
		{
			WriteClrAttributes(element, "field");
		}

		private void WritePropertyClrAttributes(IProperty element)
		{
			WriteClrAttributes(element, "property");
		}

		internal static string FieldVolatileOption(IProperty element)
		{
			return GetProperty(element, "field", "IsVolatile", "volatile");
		}

		internal static string FieldNullableOption(IProperty element)
		{
			return GetProperty(element, "field", "IsNullable", "?");
		}

		internal static string FieldConstOption(IProperty element)
		{
			return GetProperty(element, "field", "IsConst", "const");
		}

		/// <summary>
		/// Returns the 'unsafe' keword if the supplied property is marked as unsafe.
		/// </summary>
		/// <param name="element">the model element to query</param>
		/// <returns>'unsafe' keyword or empty string</returns>
		internal static string PropertyUnsafeOption(IProperty element)
		{
			return GetProperty(element, "property", "IsUnsafe", "unsafe");
		}

		/// <summary>
		/// Returns the 'unsafe' keword if the supplied interface is marked as unsafe.
		/// </summary>
		/// <param name="element">the model element to query</param>
		/// <returns>'unsafe' keyword or empty string</returns>
		internal static string InterfaceUnsafeOption(IInterface element)
		{
			return GetProperty(element, "interface", "IsUnsafe", "unsafe");
		}

		/// <summary>
		/// Checks whether the property has body according to the "HasBody" stereotype property.
		/// </summary>
		/// <param name="property">the model element to query</param>
		/// <returns>true if the property has body; false otherwise.</returns>
		internal static bool PropertyHasBody(IProperty property)
		{
			string hasBodyValue = GetProperty(property, "property", "HasBody");
			bool hasBody = false;
			if (!string.IsNullOrEmpty(hasBodyValue))
			{
				hasBody = Convert.ToBoolean(hasBodyValue);
			}

			return hasBody;
		}

		/// <summary>
		/// Checks whether the method has the params parameter according to the "params" stereotype property.
		/// </summary>
		/// <param name="property">the model element to query</param>
		/// <returns>true if the method has the params parameter; false otherwise.</returns>
		internal static bool MethodHasParams(IOperation operation)
		{
			string hasParamsValue = GetProperty(operation, "method", "params");
			bool hasParams = false;
			if (!string.IsNullOrEmpty(hasParamsValue))
			{
				hasParams = Convert.ToBoolean(hasParamsValue);
			}

			return hasParams;
		}

		internal static string EventUnsafeOption(IProperty element)
		{
			return GetProperty(element, "event", "IsUnsafe", "unsafe");
		}

		private void WriteEventClrAttributes(IProperty element)
		{
			WriteClrAttributes(element, "event");
		}

		private void WriteInterfaceClrAttributes(IInterface element)
		{
			WriteClrAttributes(element, "interface");
		}

		private void WriteClassClrAttributes(IClass element)
		{
			WriteClrAttributes(element, "class");
		}

		private void WriteStructClrAttributes(IClass element)
		{
			WriteClrAttributes(element, "struct");
		}

		private void WriteEnumClrAttributes(IEnumeration element)
		{
			WriteClrAttributes(element, "enum");
		}

		private void WriteMethodClrAttributes(IOperation element)
		{
			WriteClrAttributes(element, "method");
		}

		private void WriteIndexerClrAttributes(IOperation element)
		{
			WriteClrAttributes(element, "indexer");
		}

		internal static string MethodUnsafeOption(IOperation element)
		{
			return GetProperty(element, "method", "IsUnsafe", "unsafe");
		}

		internal static string IndexerUnsafeOption(IOperation element)
		{
			return GetProperty(element, "indexer", "IsUnsafe", "unsafe");
		}

		internal static string MethodPartialOption(IOperation element)
		{
			return GetProperty(element, "method", "IsPartial", "partial");
		}

		/// <summary>
		/// Returns the 'partial' keword if the supplied interface is marked as partial.
		/// </summary>
		/// <param name="element">the model element to query</param>
		/// <returns>'partial' keyword or empty string</returns>
		internal static string InterfacePartialOption(IInterface element)
		{
			return GetProperty(element, "interface", "IsPartial", "partial");
		}

		private void WriteClrAttributes(IElement element, string stereotypeName)
		{
			string attributeList = GetProperty(element, stereotypeName, "ClrAttributes");
			if (!string.IsNullOrEmpty(attributeList))
			{
				string trimChars = "[] \t";
				foreach (string a in attributeList.Split(';'))
				{
					WriteLine("[" + a.Trim(trimChars.ToCharArray()) + "]");
				}
			}
		}
		#endregion

		#region CSharpHelpers.t4
		/// <summary>
		/// Get the element's type if set, otherwise "object".
		/// </summary>
		/// <param name="element">the element in question</param>
		/// <param name="isEnumerable">specifies if the type should be an IEnumerable</param>
		/// <returns>the property's type</returns>
		internal static string ElementType(ITypedElement element, bool isEnumerable = false)
		{
			return ElementType(element.Type, isEnumerable);
		}

		/// <summary>
		/// Gets the type if set, otherwise "object".  
		/// </summary>
		/// <param name="type">the type in question</param>
		/// <param name="isEnumerable">specifies if the type should be an IEnumerable</param>
		/// <returns>the type as element type</returns>
		internal static string ElementType(IType type, bool isEnumerable = false)
		{
			string text = string.Empty;
			if (type == null)
			{
				text = "object";
			}
			else
			{
				text = TypeName(type);
			}

			if (!string.IsNullOrWhiteSpace(text) && isEnumerable)
			{
				text = "IEnumerable<" + text + ">";
			}

			return text;
		}

		/// <summary>
		/// Get the method's type if set, otherwise "void".
		/// </summary>
		/// <param name="operation">the operation in question</param>
		/// <returns>the operation's type</returns>
		internal static string MethodType(IOperation operation)
		{
			var parameter = operation.OwnedParameters.Where(p => p.Direction == ParameterDirectionKind.Return).FirstOrDefault();
			bool isEnumerable = false;
			if (parameter != null)
			{
				isEnumerable = IsEnumerable(parameter);
			}

			return MethodType(operation.Type, isEnumerable);
		}

		/// <summary>
		/// Get the method's type if set, otherwise "void".
		/// </summary>
		/// <param name="operation">the operation in question</param>
		/// <returns>the operation's type</returns>
		internal static string MethodType(IType type, bool isEnumerable = false)
		{
			if (type == null)
			{
				return "void";
			}

			return TypeName(type, isEnumerable);
		}

		/// <summary>
		/// Get the name of the type.
		/// </summary>
		/// <param name="type">the type in question</param>
		/// <returns>The name of the type.</returns>
		internal static string TypeName(IType type, bool isEnumerable = false)
		{
			System.Diagnostics.Debug.Assert(type != null, "type is null!");
			string text = string.Empty;
			if (type is IClassifier)
			{
				text = ClassifierName((IClassifier)type);
			}
			else
			{
				text = CSharpTranslation(type.Name);
			}

			text = text.Replace("::", ".");

			if (!string.IsNullOrWhiteSpace(text) && isEnumerable)
			{
				text = "IEnumerable<" + text + ">";
			}

			return text;
		}

		internal static bool IsMethodTypeEntity(IOperation operation)
		{
			IType type = operation.Type;
			if (type == null)
				return false;
			if (type is IClassifier)
			{
				return ((IClassifier)type).AppliedStereotypes.Any(property => property.Name == "Entity");
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Gets the name of the classifier including the template bindings or template parameters
		/// if the classifier is a templatable classifier.
		/// </summary>
		/// <param name="classifier">The classifier</param>
		/// <returns>The name of the classifier</returns>
		internal static string ClassifierName(IClassifier classifier)
		{
			var names = ClassifierProperty(element => { return GetName(element); }, GetUnboundClassifierName, AssembleName, classifier);
			return names == null ? string.Empty : names.FirstOrDefault();
		}

		/// <summary>
		/// Get the indexer's type if set, otherwise "object".
		/// </summary>
		/// <param name="operation">the operation in question</param>
		/// <returns>the indexer's type</returns>
		internal static string IndexerType(IOperation operation)
		{
			var parameter = operation.OwnedParameters.Where(p => p.Direction == ParameterDirectionKind.Return).FirstOrDefault();
			bool isEnumerable = false;
			if (parameter != null)
			{
				isEnumerable = IsEnumerable(parameter);
			}

			return ElementType(operation.Type, isEnumerable);
		}

		/// <summary>
		/// Translate a string to a C# equivalent.  For example, 
		/// CSharpTranslation("Boolean") returns "bool".  
		/// CSharpTranslation("Unrecognized text") returns "Unrecognized text".
		/// </summary>
		/// <param name="original">text to translate</param>
		/// <returns>Translated text if there is a known translation, otherwise the original text.</returns>
		internal static string CSharpTranslation(string original)
		{
			switch (original)
			{
				case "Boolean": return "bool";
				case "String": return "string";
				case "Integer": return "int";
				case "UnlimitedNatural": return "uint";
			}

			return original;
		}

		/// <summary>
		/// Defines the delegate that assembles the property of the classifier with the properties of its template parameters.
		/// </summary>
		/// <param name="classifierProperty">The property of the classifier</param>
		/// <param name="templateParameterProperties">text to translate</param>
		/// <returns>The assembled list of strings</returns>
		internal delegate List<string> Assemble(string classifierProperty, IEnumerable<string> templateParameterProperties);

		/// <summary>
		/// Gets the name of the given element
		/// </summary>
		/// <param name="namedElement">The named element</param>
		/// <returns>The c-sharp name of the element</returns>
		internal static string GetName(INamedElement namedElement)
		{
			return CSharpTranslation(namedElement.Name);
		}

		/// <summary>
		/// Implements the Assemble delegate that assembles the name of the classifier with the name of its template parameters.
		/// </summary>
		/// <param name="classifierName">The name of the classifier</param>
		/// <param name="templateParameterNames">The names of the template parameters</param>
		/// <returns>The assembled list of strings</returns>
		internal static List<string> AssembleName(string classifierName, IEnumerable<string> templateParameterNames)
		{
			List<string> names = new List<string>();
			if (!string.IsNullOrWhiteSpace(classifierName))
			{
				if (templateParameterNames.Any())
				{
					classifierName += "<" + string.Join(", ", templateParameterNames) + ">";
				}

				names.Add(classifierName);
			}

			return names;
		}


		/// <summary>
		/// Defines the delegate that assembles the property of the unbound classifier.
		/// </summary>
		/// <param name="classifierProperty">The property of the classifier</param>
		/// <param name="assemble">The delegate to assemble</param>
		/// <returns>The assembled list of strings</returns>
		internal delegate List<string> UnboundClassifierProperty(IClassifier classifier, Assemble assemble);

		/// <summary>
		/// Gets the name of the unbound classifier including the template parameters
		/// if the classifier is a templatable classifier.
		/// </summary>
		/// <param name="classifier">The classifier</param>
		/// <param name="assemble">The delegate to assemble</param>
		/// <returns>The name of the classifier</returns>
		internal static List<string> GetUnboundClassifierName(IClassifier classifier, Assemble assemble)
		{
			string classifierName = GetName(classifier);
			var templateParameterNames = TemplateParameterNames(classifier);
			return assemble(classifierName, templateParameterNames);
		}

		/// <summary>
		/// Gets the property of the classifier including the template bindings or template parameters
		/// if the classifier is a templatable classifier.
		/// </summary>
		/// <param name="getProperty">The delegate to get property we are interested</param>
		/// <param name="unboundClassifierProperty">The delegate to get the property of the unbound classifier</param>
		/// <param name="assemble">The delegate to assemble the property of the classifier with the properties of the template parameters</param>
		/// <param name="classifier">The classifier we are interested</param>
		/// <returns>The list of properties of the classifier</returns>
		internal static IEnumerable<string> ClassifierProperty(Func<INamedElement, string> getProperty, UnboundClassifierProperty unboundClassifierProperty, Assemble assemble, IClassifier classifier)
		{
			IEnumerable<ITemplateBinding> templateBindings = classifier.TemplateBindings;
			ITemplateBinding templateBinding = templateBindings.FirstOrDefault();
			if (templateBinding == null)
			{
				return unboundClassifierProperty(classifier, assemble);
			}
			else
			{
				return BoundClassifierProperty(getProperty, unboundClassifierProperty, assemble, classifier, templateBinding);
			}
		}

		/// <summary>
		/// Gets the name of the bound classifier.
		/// </summary>
		/// <param name="getProperty">The delegate to get property we are interested</param>
		/// <param name="unboundClassifierProperty">The delegate to get the property of the unbound classifier</param>
		/// <param name="assemble">The delegate to assemble the property of the classifier with the properties of the template parameters</param>
		/// <param name="classifier">The classifier we are interested</param>
		/// <param name="templateBinding">The template binding</param>
		/// <returns>The name of the classifier</returns>
		internal static List<string> BoundClassifierProperty(Func<INamedElement, string> getProperty, UnboundClassifierProperty unboundClassifierProperty, Assemble assemble, IClassifier classifier, ITemplateBinding templateBinding)
		{
			IClassifier bindingClassifier = GetBindingClassifier(templateBinding);
			if (bindingClassifier == null)
			{
				return null;
			}

			List<string> boundClassifierParameterProperties = new List<string>();
			foreach (var templateParameter in TemplateParameters(bindingClassifier))
			{
				List<string> properties = BoundElementTemplateParameterProperty(getProperty, unboundClassifierProperty, assemble, templateParameter, templateBinding);
				boundClassifierParameterProperties.AddRange(properties);
			}

			string bindingClassifierProperty = getProperty(bindingClassifier);
			return assemble(bindingClassifierProperty, boundClassifierParameterProperties);
		}

		/// <summary>
		/// Gets the name for the bound element's template parameter.
		/// </summary>
		/// <param name="getProperty">The delegate to get property we are interested</param>
		/// <param name="unboundClassifierProperty">The delegate to get the property of the unbound classifier</param>
		/// <param name="assemble">The delegate to assemble the property of the classifier with the properties of the template parameters</param>
		/// <param name="templateParameter">The template parameter</param>
		/// <param name="templateBinding">The template binding</param>
		/// <returns>The name of the template parameter</returns>
		internal static List<string> BoundElementTemplateParameterProperty(Func<INamedElement, string> getProperty, UnboundClassifierProperty unboundClassifierProperty, Assemble assemble, ITemplateParameter templateParameter, ITemplateBinding templateBinding)
		{
			List<string> properties = new List<string>();
			if (templateBinding == null)
			{
				System.Diagnostics.Debug.Fail("template binding is null!");
				return properties;
			}

			INamedElement substitution = TryGetParameterSubstitutionActual(templateParameter, templateBinding.ParameterSubstitutions);
			if (substitution == null)
			{
				properties.Add(TemplateParameterProperty(getProperty, templateParameter));
			}
			else
			{
				IClassifier substitutionClassifier = substitution as IClassifier;
				if (substitutionClassifier != null)
				{
					properties.AddRange(ClassifierProperty(getProperty, unboundClassifierProperty, assemble, substitutionClassifier));
				}
				else
				{
					properties.Add(getProperty(substitution));
				}
			}

			return properties;
		}

		/// <summary>
		/// Tries to get the actual of the parameter substitution for the given template parameter.
		/// </summary>
		/// <param name="templateParameter">The template parameter</param>
		/// <param name="parameterSubstitutions">The parameter substitutions</param>
		/// <returns>The actual of the parameter substitution for the given template parameter if any</returns>
		internal static INamedElement TryGetParameterSubstitutionActual(ITemplateParameter templateParameter, IEnumerable<ITemplateParameterSubstitution> parameterSubstitutions)
		{
			var matchingSubstitution = parameterSubstitutions.Where(substitution => substitution.Formal == templateParameter).FirstOrDefault();
			if (matchingSubstitution != null)
			{
				var actuals = matchingSubstitution.Actuals;
				if (actuals != null)
				{
					return actuals.FirstOrDefault();
				}
			}

			return null;
		}

		/// <summary>
		/// Gets the binding classifier of the given template binding.
		/// </summary>
		/// <param name="templateBinding">The template binding</param>
		/// <returns>The binding classifier</returns>
		internal static IClassifier GetBindingClassifier(ITemplateBinding templateBinding)
		{
			if (templateBinding != null)
			{
				IRedefinableTemplateSignature signature = templateBinding.Target as IRedefinableTemplateSignature;
				if (signature != null)
				{
					IClassifier bindingClassifier = signature.Classifier as IClassifier;
					System.Diagnostics.Debug.Assert(bindingClassifier != null, "binding classifier is null!");
					if (bindingClassifier != null)
					{
						return bindingClassifier;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Gets the template binding of the given classifier.
		/// </summary>
		/// <param name="classifier">The classifier</param>
		/// <returns>The template binding of the classifier if any</returns>
		internal static ITemplateBinding GetTemplateBinding(IClassifier classifier)
		{
			System.Diagnostics.Debug.Assert(classifier != null, "classifier is null!");
			if (classifier != null)
			{
				IEnumerable<ITemplateBinding> templateBindings = classifier.TemplateBindings;
				return templateBindings.FirstOrDefault();
			}

			return null;
		}

		/// <summary>
		/// Writes the constraint of the classifier according to the constrained types of 
		/// the template parameters if any. 
		/// Suppose template parameter is T, the format of the constraint is:
		///                 where T : ContrainedType
		/// </summary>
		/// <param name="classifier">The classifier</param>
		/// <returns>The constraint of the classifier</returns>
		internal void WriteClassifierConstraintOption(IClassifier classifier)
		{
			var templateParameters = ClassifierTemplateParameters(classifier);
			foreach (var templateParameter in templateParameters)
			{
				var constrainedType = templateParameter.ConstrainingClassifier;
				var parameteredElement = templateParameter.ParameteredElement;
				if (constrainedType != null && parameteredElement != null)
				{
					WriteLine("");
					PushIndent("\t");
					Write("where ");
					Write(parameteredElement.Name);
					Write(" : ");
					Write(ClassifierName(constrainedType));
					PopIndent();
				}
			}

			WriteLine("");
		}

		/// <summary>
		/// Gets the pairs of name and description for template parameters of the classifier.
		/// </summary>
		/// <param name="classifier">The classifier</param>
		/// <returns>The constraint of the classifier</returns>
		internal IEnumerable<Tuple<string, string>> TemplateParameterNameDescriptionPairs(IClassifier classifier)
		{
			var templateParameters = TemplateParameters(classifier);
			foreach (var templateParameter in templateParameters)
			{
				string parameterName = templateParameter.ParameteredElement.Name;
				string description = templateParameter.Description;
				yield return new Tuple<string, string>(parameterName, description);
			}
		}

		/// <summary>
		/// Gets the names for the template parameters of the classifier.
		/// </summary>
		/// <param name="classifier">The classifier</param>
		/// <returns>The names of the classifier's template parameters</returns>
		internal static IEnumerable<string> TemplateParameterNames(IClassifier classifier)
		{
			var templateParameters = TemplateParameters(classifier);
			return TemplateParameterNames(templateParameters);
		}

		/// <summary>
		/// Gets the names for the template parameters.
		/// </summary>
		/// <param name="templateParameters">The template parameters</param>
		/// <returns>The names of the template parameters</returns>
		internal static IEnumerable<string> TemplateParameterNames(IEnumerable<ITemplateParameter> templateParameters)
		{
			return templateParameters.Select(param => TemplateParameterName(param)).Where(name => !string.IsNullOrWhiteSpace(name));
		}

		/// <summary>
		/// Gets the name for the template parameter.
		/// </summary>
		/// <param name="templateParameters">The template parameter</param>
		/// <returns>The name of the template parameter</returns>
		internal static string TemplateParameterName(ITemplateParameter templateParameter)
		{
			return TemplateParameterProperty(element => { return GetName(element); }, templateParameter);
		}

		/// <summary>
		/// Gets the name for the template parameter.
		/// </summary>
		/// <param name="getProperty">The delegate that gets the property we are interested</param>
		/// <param name="templateParameters">The template parameter</param>
		/// <returns>The property of the template parameter</returns>
		internal static string TemplateParameterProperty(Func<INamedElement, string> getProperty, ITemplateParameter templateParameter)
		{
			if (templateParameter == null || templateParameter.OwnedParameterableElement == null)
			{
				return null;
			}

			return getProperty(templateParameter.OwnedParameterableElement);
		}

		/// <summary>
		/// Gets the constrained types for the template parameters of the classifier.
		/// </summary>
		/// <param name="classifier">The classifier</param>
		/// <returns>The names of the classifier's template parameters</returns>
		internal static IEnumerable<IClassifier> ClassifierTemplateParameterConstrainedTypes(IClassifier classifier)
		{
			var templateParameters = ClassifierTemplateParameters(classifier);
			return ClassifierTemplateParameterConstrainedTypes(templateParameters);
		}

		/// <summary>
		/// Gets the constrained types for the template parameters.
		/// </summary>
		/// <param name="templateParameters">The template parameters</param>
		/// <returns>The names of the template parameters</returns>
		internal static IEnumerable<IClassifier> ClassifierTemplateParameterConstrainedTypes(IEnumerable<IClassifierTemplateParameter> templateParameters)
		{
			return templateParameters.Where(param => param != null && param.ConstrainingClassifier != null).Select(param => param.ConstrainingClassifier);
		}

		/// <summary>
		/// Gets the classifierTemplateParameters for the classifier.
		/// </summary>
		/// <param name="classifier">The classifier</param>
		/// <returns>The classifier template parameters of the classifier</returns>
		internal static IEnumerable<IClassifierTemplateParameter> ClassifierTemplateParameters(IClassifier classifier)
		{
			var templateParameters = TemplateParameters(classifier);
			return templateParameters.Where(param => param != null && (param is IClassifierTemplateParameter)).Select(param => (IClassifierTemplateParameter)param);
		}

		/// <summary>
		/// Gets the TemplateParameters for the classifier.
		/// </summary>
		/// <param name="classifier">The classifier</param>
		/// <returns>The template parameters of the classifier</returns>
		internal static IEnumerable<ITemplateParameter> TemplateParameters(IClassifier classifier)
		{
			var templateSig = classifier.OwnedRedefinableTemplateSignature;
			if (templateSig != null)
			{
				return templateSig.OwnedParameters.Where(param => param != null);
			}

			return Enumerable.Empty<ITemplateParameter>();
		}

		#region Required Using Statements

		/// <summary>
		/// Generate using statements for the specified classifier.
		/// </summary>
		/// <param name="classifier"></param>
		internal void WriteUsingStatements(IClassifier classifier)
		{
			foreach (string namespaceName in RequiredNamespaces(classifier))
			{
				WriteLine("using " + namespaceName + ";");
			}
		}

		/// <summary>
		/// The list of using statements required for the classifier.
		/// </summary>
		/// <param name="classifier">the classifier to query</param>
		/// <returns>a list of qualified namespace names</returns>
		internal static List<string> RequiredNamespaces(IClassifier classifier)
		{
			var commonNamespaces = GetCommonlyUsedNamespaces();
			var referencedNamespaces = GetNamespacesForReferencedTypes(ReferencedTypes(classifier));
			var packageImportedNamespaces = GetNamespacesForOwningPackageImports(classifier);
			var packageUsingNamespaces = GetNamespacesForOwningPackageUsingNamespacesProperty(classifier);

			string localNamespace = GetNamespace(classifier.Namespace);
			var requiredNamespaces = RemoveAndSortNamespaces(localNamespace, commonNamespaces, referencedNamespaces, packageImportedNamespaces, packageUsingNamespaces);

			return requiredNamespaces;
		}

		/// <summary>
		/// Get a list of namespaces that being duplicates-removed and sorted.
		/// </summary>
		/// <param name="localNamespace">The local namespace</param>
		/// <param name="namespaceSets">The bunch of namespace sets</param>
		/// <returns>a list of cleaned and sorted namespaces</returns>
		internal static List<string> RemoveAndSortNamespaces(string localNamespace, params IEnumerable<string>[] namespaceSets)
		{
			var namespaces = Enumerable.Empty<string>();
			foreach (var namespaceSet in namespaceSets)
			{
				namespaces = Enumerable.Union<string>(namespaces, namespaceSet);
			}

			List<string> cleanedupNamespaces = namespaces.Where(ns => !string.IsNullOrWhiteSpace(ns)).Distinct().ToList();

			// local namespace doesn't need to show up in the using statements.
			cleanedupNamespaces.Remove(localNamespace);
			cleanedupNamespaces.Sort();

			return cleanedupNamespaces;
		}

		/// <summary>
		/// Gets the namespaces for the referenced types of the classifier.
		/// </summary>
		/// <param name="referencedTypes">the list of referenced types to query</param>
		/// <returns>a list of namespaces for the given classifier</returns>
		internal static List<string> GetNamespacesForReferencedTypes(IEnumerable<IType> referencedTypes)
		{
			List<string> namespaces = new List<string>();
			foreach (var referencedType in referencedTypes)
			{
				namespaces.AddRange(GetNamespacesForReferencedType(referencedType));
			}

			return namespaces;
		}

		/// <summary>
		/// Gets the namespaces for the referenced type.
		/// </summary>
		/// <param name="referencedType">the referenced type</param>
		/// <returns>a list of namespaces for the given type</returns>
		internal static IEnumerable<string> GetNamespacesForReferencedType(IType referencedType)
		{
			List<string> namespaces = new List<string>();
			IClassifier classifier = referencedType as IClassifier;
			if (classifier != null)
			{
				namespaces.AddRange(GetNamespacesForClassifier(classifier));
			}
			else
			{
				string namespaceName = GetNamespace(referencedType.Namespace);
				if (!string.IsNullOrWhiteSpace(namespaceName))
				{
					namespaces.Add(namespaceName);
				}
			}

			return namespaces;
		}

		/// <summary>
		/// Gets the namespaces for the classifier.
		/// </summary>
		/// <param name="classifier">the classifier to query</param>
		/// <returns>a list of namespaces for the given classifier</returns>
		internal static IEnumerable<string> GetNamespacesForClassifier(IClassifier classifier)
		{
			return ClassifierProperty(element => { return GetNamespace(element.Namespace); }, GetNamespace, AssembleNamespaces, classifier);
		}

		/// <summary>
		/// Implements the Assemble delegate that assembles the namespaces of the classifier with the namespaces of its template parameters.
		/// </summary>
		/// <param name="classifierNamespace">The namespace of the classifier</param>
		/// <param name="templateParametersNamespaces">The namespaces of the template parameters</param>
		/// <returns>The assembled list of strings</returns>
		internal static List<string> SimpleAssemble(string classifierNamespace, IEnumerable<string> templateParametersNamespaces)
		{
			List<string> namespaces = new List<string>();
			if (!string.IsNullOrWhiteSpace(classifierNamespace))
			{
				namespaces.Add(classifierNamespace);
			}

			if (templateParametersNamespaces != null && templateParametersNamespaces.Any())
			{
				namespaces.AddRange(templateParametersNamespaces);
			}

			return namespaces;
		}

		/// <summary>
		/// Implements the Assemble delegate that assembles the namespaces of the classifier with the namespaces of its template parameters.
		/// </summary>
		/// <param name="classifierNamespace">The namespace of the classifier</param>
		/// <param name="templateParametersNamespaces">The namespaces of the template parameters</param>
		/// <returns>The assembled list of strings</returns>
		internal static List<string> AssembleNamespaces(string classifierNamespace, IEnumerable<string> templateParametersNamespaces)
		{
			return SimpleAssemble(classifierNamespace, templateParametersNamespaces);
		}

		/// <summary>
		/// Gets the commonly used namespaces.
		/// </summary>
		internal static IEnumerable<string> GetCommonlyUsedNamespaces()
		{
			return new string[] { "System", "System.Collections.Generic", "System.CodeDom.Compiler", "System.Linq", "System.Runtime.Serialization", "System.ServiceModel", "System.Text", "Cqrs.Domain" };
		}

		/// <summary>
		/// Gets the list of types that the classifier references. Including parameter types, property types, 
		/// super-types, realized/inherited interfaces, navigable role type.
		/// </summary>
		/// <param name="classifier">model element to query</param>
		/// <returns>types referenced by the specified classifier</returns>
		internal static IEnumerable<IType> ReferencedTypes(IClassifier classifier)
		{
			var referencedTypes = Enumerable.Empty<IType>();

			var constrainedTypes = ContrainedTypesFromTemplateParameters(classifier);
			referencedTypes = Enumerable.Union<IType>(referencedTypes, constrainedTypes);

			var operationTypes = OperationTypes(classifier);
			referencedTypes = Enumerable.Union<IType>(referencedTypes, operationTypes);

			var attributeTypes = AttributeTypes(classifier);
			referencedTypes = Enumerable.Union<IType>(referencedTypes, attributeTypes);

			var implementedOrInheritedTypes = ImplementedOrInheritedTypes(classifier);
			referencedTypes = Enumerable.Union<IType>(referencedTypes, implementedOrInheritedTypes);

			var referencedTypesFromAssociations = ReferencedTypesFromAssociations(classifier);
			referencedTypes = Enumerable.Union<IType>(referencedTypes, referencedTypesFromAssociations);

			return referencedTypes;
		}

		/// <summary>
		/// Gets the list of types that the classifier references through template parameter constrained types.
		/// </summary>
		/// <param name="classifier">model element to query</param>
		/// <returns>types referenced by the specified classifier through constrained types of the template parameters</returns>
		internal static IEnumerable<IType> ContrainedTypesFromTemplateParameters(IClassifier classifier)
		{
			var constrainedTypes = ClassifierTemplateParameterConstrainedTypes(classifier);
			return constrainedTypes.Where(type => type != null).Select(type => (IType)type);
		}

		/// <summary>
		/// Gets the list of types that the classifier references through navigable member ends of the associations.
		/// </summary>
		/// <param name="classifier">model element to query</param>
		/// <returns>types implemented or/and inherited by the specified classifier</returns>
		internal static IEnumerable<IType> ReferencedTypesFromAssociations(IClassifier classifier)
		{
			var navigableMemberEndsTypes = classifier.GetRelatedLinks<IAssociation>().SelectMany(assoc => assoc.NavigableOwnedEnds.Select(end => end.Type));
			return navigableMemberEndsTypes.Where(type => type != null);
		}

		/// <summary>
		/// Gets the list of types that the classifier references in the operation.
		/// </summary>
		/// <param name="classifier">model element to query</param>
		/// <returns>types referenced by the specified classifier's operations</returns>
		internal static IEnumerable<IType> OperationTypes(IClassifier classifier)
		{
			// note: return type is actually one of the owned parameters.
			var parameterTypes = classifier.OwnedElements.OfType<IOperation>().SelectMany(op => op.OwnedParameters.Select(param => param.Type));
			return parameterTypes.Where(type => type != null);
		}

		/// <summary>
		/// Gets the list of types that the classifier references in attributes.
		/// </summary>
		/// <param name="classifier">model element to query</param>
		/// <returns>types referenced by the specified classifier's attributes</returns>
		internal static IEnumerable<IType> AttributeTypes(IClassifier classifier)
		{
			var propertyTypes = classifier.OwnedElements.OfType<IProperty>().Select(attrib => attrib.Type);
			return propertyTypes.Where(type => type != null);
		}

		/// <summary>
		/// Gets the list of types that the classifier references. Including super-types, realized/inherited interfaces.
		/// </summary>
		/// <param name="classifier">model element to query</param>
		/// <returns>types implemented or/and inherited by the specified classifier</returns>
		internal static IEnumerable<IType> ImplementedOrInheritedTypes(IClassifier classifier)
		{
			var implementedOrInheritedTypes = Enumerable.Empty<IType>();
			if (classifier is IInterface)
			{
				implementedOrInheritedTypes = ((IInterface)classifier).Generals.OfType<IType>().Where(type => type != null);
			}
			else if (classifier is IClass)
			{
				implementedOrInheritedTypes = ImplementedAndInheritedTypes((IClass)classifier);
			}

			return implementedOrInheritedTypes;
		}

		/// <summary>
		/// Get the implemented and inherited types of the given class
		/// </summary>
		/// <param name="aClass">The given class</param>
		/// <returns>implementedAndInherted types</returns>
		internal static IEnumerable<IType> ImplementedAndInheritedTypes(IClass aClass)
		{
			// if aClass is stereotyped as a "struct", we should ignore superclasses;
			bool isStruct = GetStereotype(aClass) == "struct";
			if (isStruct)
			{
				return aClass.InterfaceRealizations.Select(ir => ir.Contract).Where(type => type != null);
			}
			var implementedAndInherited = Enumerable.Union<IType>(aClass.SuperClasses, aClass.InterfaceRealizations.Select(ir => ir.Contract));
			return implementedAndInherited.Where(type => type != null);
		}

		/// <summary>
		/// Gets the namespace names that the specified package (and all of its ancestor packages) imports.
		/// </summary>
		/// <param name="package">the package to query</param>
		internal static List<string> GetNamespacesForOwningPackageImports(IClassifier classifier)
		{
			List<string> namespaces = new List<string>();
			NamespacesForPackageImports(namespaces, classifier.OwningPackage);

			return namespaces;
		}

		/// <summary>
		/// Gets the namespace names that the specified package (and all of its ancestor packages) 
		/// imports. This is a recursive call.
		/// </summary>
		/// <param name="package">the package to query</param>
		internal static void NamespacesForPackageImports(List<string> namespaces, IPackage owningPackage)
		{
			if (owningPackage == null)
			{
				return;
			}

			// Get the namespaces for the packages imported by classifier's containing package.
			foreach (IPackageImport packageImport in owningPackage.PackageImports)
			{
				string namespaceName = GetNamespace(packageImport.ImportedPackage);
				if (!string.IsNullOrWhiteSpace(namespaceName))
				{
					namespaces.Add(namespaceName);
				}
			}

			NamespacesForPackageImports(namespaces, owningPackage.OwningPackage);
		}

		/// <summary>
		/// Gets the namespace names that the classifier's owning packages have in its UsingNamesspaces stereotype property.
		/// </summary>
		/// <param name="package">the package to query</param>
		internal static List<string> GetNamespacesForOwningPackageUsingNamespacesProperty(IClassifier classifier)
		{
			List<string> namespaces = new List<string>();
			NamespacesForOwningPackageUsingNamespacesProperty(namespaces, classifier.OwningPackage);

			return namespaces;
		}


		/// <summary>
		/// Gets the namespace names that the specified package (and all of its ancestor packages) 
		/// names in its UsingNamesspaces stereotype property.  This is a recursive call.
		/// </summary>
		/// <param name="package">the package to query</param>
		internal static void NamespacesForOwningPackageUsingNamespacesProperty(List<string> namespaces, IPackage owningPackage)
		{
			if (owningPackage == null)
			{
				return;
			}

			// Get the namespaces identified in the [C# namespace].UsingNamespaces property of the classifier's containing package.
			string usingNamespaces = owningPackage.GetStereotypeProperty(CSharpConstants.ProfileName, CSharpConstants.NamespaceStereotypeName, CSharpConstants.UsingNamespacesStereotypePropertyName);
			if (usingNamespaces != null)
			{
				foreach (string item in usingNamespaces.Split(new char[] { ',', ';' }))
				{
					string namespaceName = item.Trim();
					if (!string.IsNullOrWhiteSpace(namespaceName))
					{
						namespaces.Add(namespaceName);
					}
				}
			}

			NamespacesForOwningPackageUsingNamespacesProperty(namespaces, owningPackage.OwningPackage);
		}

		#endregion Required Using Statements

		#region GetNamespace

		/// <summary>
		/// Get the namespace string for the given namespaceElement.
		/// Note: we assume root model stands for the global namespace therefore
		///       we won't show root model in the namespace.
		/// </summary>
		/// <param name="namespaceElement">The namespace element</param>
		/// <returns>The namespace string of the given namespaceElement</returns>
		internal static string GetNamespace(INamespace namespaceElement)
		{
			return string.Join(".", NamespaceHierarchy(namespaceElement).Select(ns => ns.Name));
		}

		/// <summary>
		/// Implements the delegate UnboundClassifierProperty to get namespace of the given classifier.
		/// </summary>
		/// <param name="classifier">The classifier</param>
		/// <param name="assemble">The delegate to assemble</param>
		/// <returns>The namespace of the given classifier</returns>
		internal static List<string> GetNamespace(IClassifier classifier, Assemble assemble)
		{
			List<string> classifierNamespace = new List<string>();
			string aNamespace = GetNamespace(classifier.Namespace);
			if (!string.IsNullOrWhiteSpace(aNamespace))
			{
				classifierNamespace.Add(aNamespace);
			}

			return classifierNamespace;
		}

		/// <summary>
		/// Get a collection of namespaces in the hierarchy of the given namespace element.
		/// Note: we assume root model stands for the global namespace therefore
		///       it is not included in the collection.
		/// </summary>
		/// <param name="namespaceElement">The namespace element</param>
		/// <returns>The collection of namespaces of the given namespaceElement hierarchy</returns>
		internal static IEnumerable<INamespace> NamespaceHierarchy(INamespace namespaceElement)
		{
			if (namespaceElement == null)
			{
				return Enumerable.Empty<INamespace>();
			}

			if (namespaceElement.Namespace == null)
			{
				return Enumerable.Empty<INamespace>();
			}
			else
			{
				return Enumerable.Union(NamespaceHierarchy(namespaceElement.Namespace), new[] { namespaceElement });
			}
		}

		/// <summary>
		/// Get the namespace string of the parent for the given classifier.
		/// Note: we assume root model stands for the global namespace therefore
		///       we won't show root model in the namespace.
		/// </summary>
		/// <param name="classifier">The classifier</param>
		/// <returns>The name of the classifier</returns>
		internal static string GetParentNamespace(IClassifier classifier)
		{
			return GetNamespace(classifier.OwningPackage.Namespace);
		}

		#endregion GetNamespace

		/// <summary>
		/// Write the property accessor body.
		/// </summary>
		/// <param name="operation">The operation</param>
		internal void WritePropertyAccessorBody(IProperty property)
		{
			if (PropertyHasBody(property))
			{
				WriteLine("");
				WriteDefaultImplementation();
			}
			else
			{
				Write(";");
				WriteLine("");
			}
		}

		/// <summary>
		/// Write the default implementation of method, getter, or setters.
		/// </summary>
		internal void WriteDefaultImplementation()
		{
			WriteLine("");
			WriteLine("{");
			PushIndent("\t");
			WriteLine("throw new System.NotImplementedException();");
			PopIndent();
			WriteLine("}");
		}

		/// <summary>
		/// Write the parameter list for the given method.
		/// </summary>
		/// <param name="operation">The operation</param>
		/// <param name="separator">Optional, ", " by default</param>
		internal void WriteMethodParameterList(IOperation operation, string separator = ", ")
		{
			bool hasParams = MethodHasParams(operation);
			WriteParameterList(operation, separator, hasParams);
		}

		/// <summary>
		/// Write the parameter list for the given indexer.
		/// </summary>
		/// <param name="operation">The operation</param>
		/// <param name="separator">Optional, ", " by default</param>
		internal void WriteIndexerParameterList(IOperation operation, string separator = ", ")
		{
			WriteParameterList(operation, separator);
		}

		/// <summary>
		/// Write the parameter list for the given operation.
		/// </summary>
		/// <param name="operation">The operation</param>
		/// <param name="separator">Optional, ", " by default</param>
		internal void WriteParameterList(IOperation operation, string separator = ", ", bool hasParam = false)
		{
			var parameters = operation.OwnedParameters.Where(p => p.Direction != ParameterDirectionKind.Return);
			int count = parameters.Count();
			for (int i = 0; i < count; i++)
			{
				var p = parameters.ElementAt(i);

				if (i > 0)
				{
					Write(separator);
				}

				Write(ParameterDirection(p));

				bool isParams = hasParam && (i == count - 1);
				if (isParams)
				{
					Write("params ");
				}

				bool isEnumerable = IsEnumerable(p);
				Write(ElementType(p, isEnumerable));

				if (isParams)
				{
					Write("[]");
				}

				Write(" ");
				Write(p.Name);
				WriteParameterDefaultValueOption(p, isEnumerable);
			}
		}

		/// <summary>
		/// Determine if the property's multiplicity setting requires an IEnumerable type"/>
		/// </summary>
		/// <param name="property">query this element's multiplicity</param>
		/// <returns>true if the element's multiplicity specifies an upper bound greater than 1</returns>
		internal static bool IsEnumerable(IProperty property)
		{
			return IsEnumerable(property.UpperValue);
		}

		/// <summary>
		/// Determine if the parameter's multiplicty setting requires an IEnumerable type"/>
		/// </summary>
		/// <param name="parameter">query this element's multiplicity</param>
		/// <returns>true if the element's multiplicity specifies an upper bound greater than 1</returns>
		internal static bool IsEnumerable(IParameter parameter)
		{
			return IsEnumerable(parameter.UpperValue);
		}

		/// <summary>
		/// Determine if the provided value represents an upper bound that requires an IEnumerable type"/>
		/// </summary>
		/// <param name="upperValue">value representing a multiplicity's upper bound</param>
		/// <returns>true if the upperValue represents a value greater than 1</returns>
		internal static bool IsEnumerable(IValueSpecification upperValue)
		{
			if (upperValue != null)
			{
				string upperBound = upperValue.ToString();
				if (!string.IsNullOrWhiteSpace(upperBound))
				{
					if (upperBound.Contains("*"))
					{
						return true;
					}
					int val;
					if (int.TryParse(upperBound, out val))
					{
						return val > 1;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Write the default value of the parameter
		/// </summary>
		/// <param name="parameter">The parameter</param>
		/// <param name="isEnumerable">Whether the parameter is an enumerable.</param>
		private void WriteParameterDefaultValueOption(IParameter parameter, bool isEnumerable)
		{
			if (parameter == null)
			{
				return;
			}

			WriteDefaultValue(parameter.Type, parameter.DefaultValue, parameter.Default, isEnumerable);
		}

		/// <summary>
		/// Write field default value string
		/// </summary>
		/// <param name="field">The field</param>
		/// <param name="isEnumerable">Whether the field is an enumerable.</param>
		private void WriteFieldDefaultValueOption(IProperty field, bool isEnumerable)
		{
			if (field == null)
			{
				return;
			}

			bool isConst = !string.IsNullOrEmpty(FieldConstOption(field));
			WriteDefaultValue(field.Type, field.DefaultValue, field.Default, isEnumerable, isConst);
		}

		/// <summary>
		/// Write the default value.
		/// </summary>
		/// <param name="type">The given type</param>
		/// <param name="defaultValue">The default value.</param>
		/// <param name="defaultValueString">The default value in string.</param>
		/// <param name="isEnumerable">Whether the type is an enumerable.</param>
		/// <param name="isEnumerable">Whether the type is an const.</param>
		private void WriteDefaultValue(IType type, IValueSpecification defaultValue, string defaultStringValue, bool isEnumerable, bool isConst = false)
		{
			if (!isConst && ((defaultValue == null) || string.IsNullOrEmpty(defaultStringValue)))
			{
				return;
			}

			if (isConst && ((defaultValue == null) || string.IsNullOrEmpty(defaultStringValue)))
			{
				defaultStringValue = string.Empty;
				defaultStringValue += "default(" + ElementType(type) + ")";
				defaultStringValue += " /* TODO: specify a default. You can define this by setting the DefaultValue stereotype property of the attribute. */";
			}
			else if (!isEnumerable && type != null && type.Name == "String")
			{
				defaultStringValue = "\"" + defaultStringValue + "\"";
			}
			else if (!isEnumerable && type != null && type.Name == "Boolean")
			{
				defaultStringValue = ((ILiteralBoolean)defaultValue).Value == true ? "true" : "false";
			}

			Write(" = " + defaultStringValue);
		}

		/// <summary>
		/// Translate the parameter direction kind into sharp keyword.
		/// </summary>
		/// <param name="parameter">The parameter</param>
		/// <returns>Translated text if there is a known translation, otherwise the direction ToString().</returns>
		internal static string ParameterDirection(IParameter parameter)
		{
			if (parameter == null)
			{
				return string.Empty;
			}

			switch (parameter.Direction)
			{
				case ParameterDirectionKind.In:
					return string.Empty;
				case ParameterDirectionKind.InOut:
					return "ref ";
				case ParameterDirectionKind.Out:
					return "out ";
				case ParameterDirectionKind.Return:
					return string.Empty;
			}

			return parameter.Direction.ToString() + " ";
		}

		#region Common Section for Attribute (Field/Property), Operation (Method)

		/// <summary>
		/// Write UmlProperty definition.
		/// </summary>
		/// <param name="umlProperty">The uml IProperty</param>
		/// <param name="overloadOption">Overload keyword (optional)</param>
		private void WriteUmlPropertyDefinition(IProperty umlProperty, string overloadOption = "")
		{
			if (GetStereotype(umlProperty) == "field" || GetStereotype(umlProperty) == "event")
			{
				WriteFieldClrAttributes(umlProperty);
				WriteFieldDefinition(umlProperty);
			}
			else if (GetStereotype(umlProperty) == "property")
			{
				WritePropertyClrAttributes(umlProperty);
				WritePropertyDefinition(umlProperty, overloadOption);
			}
		}

		/// <summary>
		/// Write a property definition appropriate for class or struct.
		/// </summary>
		/// <param name="property">The umlproperty as a property.</param>
		/// <param name="overloadOption">Overload keyword (optional)</param>
		private void WritePropertyDefinition(IProperty property, string overloadOption = "")
		{
			Write(PropertyUnsafeOption(property));
			Write(PropertyVisibility(property));
			Write(AttributeStaticOption(property));
			if (!string.IsNullOrEmpty(overloadOption))
			{
				Write(overloadOption);
			}

			WritePropertyTypeAndVariableName(property);
			WriteLine("{");
			PushIndent("\t");
			Write(PropertyGetVisibility(property));
			Write("get");
			WritePropertyAccessorBody(property);

			if (property.IsReadOnly)
			{
				Write("private ");
			}
			else
			{
				Write(PropertySetVisibility(property));
			}

			Write("set");
			WritePropertyAccessorBody(property);

			PopIndent();
			WriteLine("}");
		}

		/// <summary>
		/// Write a field definition.
		/// </summary>
		/// <param name="field">The umlproperty as a field.</param>
		private void WriteFieldDefinition(IProperty field)
		{
			Write(FieldVolatileOption(field));
			Write(FieldVisibility(field));
			Write(AttributeStaticOption(field));
			Write(FieldReadOnlyOption(field));
			Write(FieldConstOption(field));
			if (GetStereotype(field) == "event")
			{
				Write("event ");
			}
			WriteFieldTypeAndVariableName(field);
		}

		/// <summary>
		/// Write the field type and variable name, and possibly default value
		/// </summary>
		/// <param name="property">The property</param>
		private void WriteFieldTypeAndVariableName(IProperty property)
		{
			string typeName = ElementType(property);
			string fieldName = property.Name;
			if (!string.IsNullOrEmpty(typeName) && !string.IsNullOrEmpty(fieldName))
			{
				bool isEnumerable = IsEnumerable(property);
				if (!isEnumerable)
				{
					Write(typeName);
					Write(FieldNullableOption(property) == string.Empty ? " " : FieldNullableOption(property));
					Write(fieldName);
				}
				else
				{
					Write(ElementType(property, isEnumerable));
					Write(" ");
					Write(fieldName);
				}

				WriteFieldDefaultValueOption(property, isEnumerable);
				Write(";");
				WriteLine("");
			}
		}

		/// <summary>
		/// Write the property type and variable name.
		/// </summary>
		/// <param name="property">The property</param>
		/// <param name="isEndOfLine">true to end the line</param>
		private void WritePropertyTypeAndVariableName(IProperty property, bool isEndOfLine = true)
		{
			string typeName = ElementType(property);
			string propertyName = property.Name;
			if (!string.IsNullOrEmpty(typeName) && !string.IsNullOrEmpty(propertyName))
			{
				Write(ElementType(property, IsEnumerable(property)) + " " + propertyName);

				if (isEndOfLine)
				{
					WriteLine("");
				}
			}
		}

		/// <summary>
		/// Check if an attribute exists with the same name and type as the member end.
		/// If so, and if the visibility on the attribute 
		/// </summary>
		/// <param name="memberEnd">The member end</param>
		/// <param name="attributes">The attributes</param>
		/// <returns>true if the attribute with the same name and type as memberEnd is already processed.</returns>
		internal static bool IsMemberEndProcessedAsAttribute(IProperty memberEnd, IEnumerable<IProperty> attributes)
		{
			bool processed = false;
			var existingAttribs = attributes.Where(attrib => StringComparer.Ordinal.Equals(memberEnd.Name, attrib.Name) && (memberEnd.Type == attrib.Type));
			if (existingAttribs.Any())
			{
				processed = true;

				// check if the visibility conflicts, if so, send the user the warning message.
				IProperty existingAttrib = existingAttribs.FirstOrDefault();
				if (existingAttrib.Visibility != memberEnd.Visibility)
				{
					// Send user the warning message.
				}
			}

			return processed;
		}

		/// <summary>
		/// Operation static string
		/// </summary>
		/// <param name="operation">The operation</param>
		/// <returns>The static option string for operation</returns>
		internal static string OperationStaticOption(IOperation operation)
		{
			return operation.IsStatic ? "static " : string.Empty;
		}

		/// <summary>
		/// Field read-only string
		/// </summary>
		/// <param name="field">The field</param>
		/// <returns>The read-only option string for field</returns>
		internal static string FieldReadOnlyOption(IProperty field)
		{
			return field.IsReadOnly ? "readonly " : string.Empty;
		}

		/// <summary>
		/// Attribute static string
		/// </summary>
		/// <param name="attribute">The attribute</param>
		/// <returns>The static option string for attribute</returns>
		internal static string AttributeStaticOption(IProperty attribute)
		{
			return attribute.IsStatic ? "static " : string.Empty;
		}

		/// <summary>
		/// Operation sealed string
		/// </summary>
		/// <param name="operation">The operation</param>
		/// <returns>The static option string for operation</returns>
		internal static string OperationSealedOption(IOperation operation)
		{
			return operation.IsLeaf ? "sealed " : string.Empty;
		}

		/// <summary>
		/// Operation abstract string
		/// </summary>
		/// <param name="operation">The operation</param>
		/// <returns>The abstract option string for operation</returns>
		internal static string OperationAbstractOption(IOperation operation)
		{
			return operation.IsAbstract ? "abstract " : string.Empty;
		}

		/// <summary>
		/// Property sealed string
		/// </summary>
		/// <param name="property">The property</param>
		/// <returns>The sealed option string for property</returns>
		internal static string PropertySealedOption(IProperty property)
		{
			return property.IsLeaf ? "sealed " : string.Empty;
		}

		/// <summary>
		/// Check if the operation is a constructor.
		/// </summary>
		/// <param name="operation">The operation</param>
		/// <returns>true if the operation is a constructor</returns>
		internal static bool IsConstructor(IOperation operation)
		{
			if (operation.Owner is IClassifier)
			{
				IClassifier owner = (IClassifier)operation.Owner;
				return operation.Name == owner.Name;
			}

			return false;
		}

		/// <summary>
		/// Gets the navigable owed ends which not correspond to any owned attributes for the given classifier.
		/// </summary>
		/// <param name="classifier">The classifier</param>
		/// <param name="ownedAttributes">The owned attributes</param>
		/// <returns>The owned attributes</returns>
		internal static IEnumerable<IProperty> GetNavigableOwnedEnds(IClassifier classifier, IEnumerable<IProperty> ownedAttributes)
		{
			foreach (IAssociation association in classifier.GetRelatedLinks<IAssociation>())
			{
				IEnumerable<IProperty> navigableOwnedEnds = association.NavigableOwnedEnds;
				foreach (IProperty ownedEnd in navigableOwnedEnds)
				{
					if ((!StringComparer.OrdinalIgnoreCase.Equals(ownedEnd.Type.QualifiedName, classifier.QualifiedName)
						|| association.SourceElement == association.TargetElement)
						&& !IsMemberEndProcessedAsAttribute(ownedEnd, ownedAttributes))
					{
						yield return ownedEnd;
					}
				}
			}
		}
		#endregion Common Section for Attribute (Field/Property), Operation (Method)
		#endregion

		#region ModelHelper
		internal static IEnumerable<IElement> AllElements(IElement t)
		{
			if (!ShouldGenerateCodeForElement(t))
			{
				yield break;
			}

			yield return t;

			if (t.OwnedElements != null && t.OwnedElements.Any())
			{
				foreach (var elem in t.OwnedElements)
				{
					foreach (var child in AllElements(elem))
					{
						yield return child;
					}
				}
			}
		}

		internal static IEnumerable<IElement> AllElements(IElement t, params string[] stereotypes)
		{
			if (!ShouldGenerateCodeForElement(t))
			{
				yield break;
			}

			var elementStereotypes = t.AppliedStereotypes.Select(s => s.Name);
			if (elementStereotypes != null && elementStereotypes.Intersect(stereotypes).Any())
			{
				yield return t;
			}

			if (t.OwnedElements != null && t.OwnedElements.Any())
			{
				foreach (var elem in t.OwnedElements)
				{
					foreach (var child in AllElements(elem, stereotypes))
					{
						yield return child;
					}
				}
			}
		}

		internal static string FindDomainName(IElement t)
		{
			var domain = FindDomain(t) as INamedElement;

			return domain == null ? string.Empty : domain.Name;
		}

		internal static IElement FindDomain(IElement t)
		{
			var domainStereoType = t.AppliedStereotypes.FirstOrDefault(s => s.Name == "Domain");
			if (domainStereoType != null)
			{
				return (t as INamedElement);
			}
			else if (t.Owner != null)
			{
				return FindDomain(t.Owner);
			}
			else
			{
				return null;
			}
		}

		internal static string GetStereotypeValue(IElement element, string stereotypeName, string parameterName)
		{
			var property = element.AppliedStereotypes
				.Single(p => p.Name.Equals("Command", StringComparison.InvariantCultureIgnoreCase))
				.PropertyInstances
				.Single(p => p.Name.Equals(parameterName, StringComparison.InvariantCultureIgnoreCase));

			return property.Value;
		}
		#endregion

		#region CqrsTemplateHelpers
		private string TemplateHelperToolName
		{
			get { return "CQRS UML Code Generator"; }
		}

		private string TemplateHelperVersionNumber
		{
			get { return "1.500.0.1"; }
		}

		private string WebsiteURL
		{
			get { return "https://cqrs.co.nz"; }
		}

		private string DataContractVersionNumber
		{
			get { return "1001"; }
		}

		internal static List<string> GetNamespaces(IClassifier classifier, params string[] extraNamespaces)
		{
			List<string> namespaces = new List<string>();
			namespaces.AddRange(extraNamespaces);
			namespaces.AddRange(RequiredNamespaces(classifier));
			namespaces = namespaces.OrderByDescending(p => p.StartsWith("System")).ThenBy(p => p).Distinct().ToList();
			return namespaces;
		}

		private string GetDataContractAttribute(IClassifier classifier)
		{
			string urlNamespace = "/";
			string classNamespace = GetNamespace(classifier.Namespace);
			if (!string.IsNullOrEmpty(classNamespace))
			{
				var nsSplit = classNamespace.Split('.');
				nsSplit.Skip(1).ToList().ForEach(p => urlNamespace += p + "/");
			}

			return string.Format("[DataContract(Namespace=\"{0}{1}{2}/\")]", WebsiteURL, urlNamespace, DataContractVersionNumber);
		}

		private string GetServiceContractAttribute(IClassifier classifier)
		{
			string urlNamespace = "/";
			string classNamespace = GetNamespace(classifier.Namespace);
			if (!string.IsNullOrEmpty(classNamespace))
			{
				var nsSplit = classNamespace.Split('.');
				nsSplit.Skip(1).ToList().ForEach(p => urlNamespace += p + "/");
			}

			return string.Format("[ServiceContract(Namespace=\"{0}{1}{2}/\")]", WebsiteURL, urlNamespace, DataContractVersionNumber);
		}

		private string GetGeneratedCodeAttribute()
		{
			return string.Format("[GeneratedCode(\"{0}\", \"{1}\")]", TemplateHelperToolName, TemplateHelperVersionNumber);
		}

		private string GetDisplayType(ITypedElement typedElement)
		{
			string typeName;

			if (typedElement == null)
				return string.Empty;
			else if (typedElement.Type == null)
				return "object";
			else if (typedElement is IClassifier)
				typeName = ClassifierName((IClassifier)typedElement.Type);
			else
				typeName = CSharpTranslation(typedElement.Type.Name);

			return typeName;
		}

		private string GetBaseClass(IClassifier element)
		{
			// It should only ever have one base class
			return
				element.Generalizations.Count() <= 0
				? string.Empty
				: element.Generalizations.First().General.Name;
		}

		private string GetAbstract(IClass element)
		{
			return element.IsAbstract ? " abstract" : string.Empty;
		}

		private string GetFullDisplayType(IMultiplicityElement property)
		{
			return this.GetFullDisplayType(property, true);
		}

		private string GetFullDisplayType(IMultiplicityElement property, bool onlyStructAsNullable)
		{
			var typedProperty = property as ITypedElement;

			string typeName = string.Empty;
			bool canBeEmpty = false;
			bool isUnique = false;
			bool canBeMultiple = false;
			bool isStruct = false;
			bool isEnum = false;
			bool isEntity = false;

			typeName = ElementType(typedProperty);
			if (typeName.StartsWith("Root::"))
			{
				typeName = typeName.Substring(6).Replace("::", ".");
			}
			canBeEmpty = property.LowerValue != null ? property.LowerValue.ToString() == "0" : false;
			isUnique = property.IsUnique;
			canBeMultiple = property.UpperValue != null ? property.UpperValue.ToString() == "*" : false;
			isStruct = typedProperty != null && typedProperty.Type != null && (typedProperty.Type.AppliedStereotypes.Any(p => p.Name.Equals("Struct", StringComparison.InvariantCultureIgnoreCase)) || typeName == "int" || typeName == "integer");
			isEnum = (typedProperty.Type as IEnumeration) != null;
			isEntity = typedProperty != null && typedProperty.Type != null && typedProperty.Type.AppliedStereotypes.Any(p => p.Name.Equals("Entity", StringComparison.InvariantCultureIgnoreCase));

			return string.Format("{0}{1}{2}{3}{4}"
				, canBeMultiple && isUnique ? "ICollection<" : canBeMultiple ? "IEnumerable<" : string.Empty
				, typeName
				, isEntity ? "Entity" : string.Empty
				, canBeMultiple ? ">" : string.Empty
				, !canBeMultiple && canBeEmpty && (!onlyStructAsNullable || isStruct || isEnum) ? "?" : string.Empty);
		}

		private string GetMethodParameterList(IOperation operation)
		{
			var methodParameterList = string.Empty;

			var parameters = operation.OwnedParameters.Where(p => p.Direction != ParameterDirectionKind.Return).ToList();
			for (int i = 0; i < parameters.Count(); i++)
			{
				IParameter parameter = parameters.ElementAt(i);
				if (i > 0)
					methodParameterList += ", ";
				methodParameterList += (GetFullDisplayType(parameter, false) + " " + parameter.Name);
				if (parameter.DefaultValue != null && !string.IsNullOrWhiteSpace(parameter.DefaultValue.ToString()))
					methodParameterList += " = " + parameter.DefaultValue;
			}

			return methodParameterList;
		}

		private string GetMethodParameterNameList(IOperation operation)
		{
			var methodParameterList = string.Empty;

			var parameters = operation.OwnedParameters.Where(p => p.Direction != ParameterDirectionKind.Return);
			for (int i = 0; i < parameters.Count(); i++)
			{
				IParameter parameter = parameters.ElementAt(i);
				if (i > 0)
					methodParameterList += ", ";
				methodParameterList += parameter.Name;
			}

			return methodParameterList;
		}

		private string GetMethodParameterTypeList(IOperation operation)
		{
			var methodParameterList = string.Empty;

			var parameters = operation.OwnedParameters.Where(p => p.Direction != ParameterDirectionKind.Return);
			for (int i = 0; i < parameters.Count(); i++)
			{
				IParameter parameter = parameters.ElementAt(i);
				if (i > 0)
					methodParameterList += ", ";
				methodParameterList += (GetFullDisplayType(parameter, false));
			}

			return methodParameterList;
		}

		private List<IOperation> GetMethodList(IClass classElement)
		{
			var operations = new List<IOperation>();
			operations.AddRange(classElement.OwnedOperations);
			return operations;
		}

		private string GetMethodVisibility(IOperation operation)
		{
			switch (operation.Visibility)
			{
				case (Microsoft.VisualStudio.Uml.Classes.VisibilityKind.Public):
					return "public";
				case (Microsoft.VisualStudio.Uml.Classes.VisibilityKind.Private):
					return "private";
				case (Microsoft.VisualStudio.Uml.Classes.VisibilityKind.Protected):
					return "protected";
				case (Microsoft.VisualStudio.Uml.Classes.VisibilityKind.Package):
					return "internal";
				default:
					return "public";
			}
		}

		internal static List<IProperty> GetPropertyList(IClass classElement)
		{
			var properties = new List<IProperty>();
			properties.AddRange(classElement.OwnedAttributes);
			properties.AddRange(GetNavigableOwnedEnds(classElement, classElement.OwnedAttributes));
			return properties;
		}

		private string GetPropertyVisibility(IProperty property)
		{
			switch (property.Visibility)
			{
				case (Microsoft.VisualStudio.Uml.Classes.VisibilityKind.Public):
					return "public";
				case (Microsoft.VisualStudio.Uml.Classes.VisibilityKind.Private):
					return "private";
				case (Microsoft.VisualStudio.Uml.Classes.VisibilityKind.Protected):
					return "protected";
				case (Microsoft.VisualStudio.Uml.Classes.VisibilityKind.Package):
					return "internal";
				default:
					return "public";
			}
		}

		private bool IsInheritingInModel(IClass classElement)
		{
			return classElement.Generalizations.Count() > 0;
		}

		internal static bool ShouldGenerateCodeForElement(IElement element)
		{
			var currentElement = element;
			while (currentElement != null)
			{
				if (currentElement.AppliedStereotypes.Any(p => p.Name.Equals("Proposed", StringComparison.OrdinalIgnoreCase)))
					return false;
				if (currentElement.AppliedStereotypes.Any(p => p.Name.Equals("AutoGenerated", StringComparison.OrdinalIgnoreCase)))
					return false;

				currentElement = currentElement.Owner;
			}

			return true;
		}

		internal static IElement FindDomainElement(IElement t)
		{
			var domainStereoType = t.AppliedStereotypes.FirstOrDefault(s => s.Name == "Domain");
			if (domainStereoType != null)
			{
				return (t as INamedElement);
			}
			else if (t.Owner != null)
			{
				return FindDomainElement(t.Owner);
			}
			else
			{
				return null;
			}
		}

		internal static string GetEntityPersistenceTechnology(IElement t)
		{
			IElement domainElement = FindDomainElement(t);
			var domainStereoTypeProperties = domainElement.AppliedStereotypes.Single(property => property.Name == "Domain");
			return domainStereoTypeProperties.PropertyInstances.Single(property => property.Name == "EntityPersistenceTechnology").Value;
		}

		internal static string GetCommandHandlerTechnology(IElement t)
		{
			IElement domainElement = FindDomainElement(t);
			var domainStereoTypeProperties = domainElement.AppliedStereotypes.Single(property => property.Name == "Domain");
			return domainStereoTypeProperties.PropertyInstances.Single(property => property.Name == "CommandHandlerTechnology").Value;
		}

		internal static string GetEventHandlerTechnology(IElement t)
		{
			IElement domainElement = FindDomainElement(t);
			var domainStereoTypeProperties = domainElement.AppliedStereotypes.Single(property => property.Name == "Domain");
			return domainStereoTypeProperties.PropertyInstances.Single(property => property.Name == "EventHandlerTechnology").Value;
		}

		internal static string GetAggregateTechnology(IElement t)
		{
			IElement domainElement = FindDomainElement(t);
			var domainStereoTypeProperties = domainElement.AppliedStereotypes.Single(property => property.Name == "Domain");
			return domainStereoTypeProperties.PropertyInstances.Single(property => property.Name == "AggregateTechnology").Value;
		}

		internal static string GetDomainAuthenticationTokenType(IElement t)
		{
			IElement domainElement = FindDomainElement(t);
			var domainStereoTypeProperties = domainElement.AppliedStereotypes.Single(property => property.Name == "Domain");
			return domainStereoTypeProperties.PropertyInstances.Single(property => property.Name == "AuthenticationTokenType").Value;
		}

		internal static IElement FindModuleElement(IElement t)
		{
			var domainStereoType = t.AppliedStereotypes.FirstOrDefault(s => s.Name == "Module");
			if (domainStereoType != null)
			{
				return (t as INamedElement);
			}
			else if (t.Owner != null)
			{
				return FindModuleElement(t.Owner);
			}
			else
			{
				return null;
			}
		}

		private INamedElement GetRoot(IElement t)
		{
			var result = t;
			if (result.Owner != null)
			{
				do
				{
					result = result.Owner;
				}
				while (result.Owner != null);
			}

			return result as INamedElement;
		}

		private INamedElement GetElementByFullName(IElement elt, string elementName)
		{
			INamedElement currentElement;
			if (elementName.Contains("::"))
				currentElement = GetRoot(elt);
			else
			{
				currentElement = FindModuleElement(elt) as INamedElement;
				currentElement =
					currentElement.OwnedElements
					.FirstOrDefault(e => e is INamedElement && ((INamedElement)e).Name.Equals(elementName, StringComparison.InvariantCultureIgnoreCase)) as INamedElement;

				return currentElement;
			}

			var pathElements = elementName.Split(new string[] { "::" }, StringSplitOptions.None);

			if (pathElements.Length < 2)
				return null;

			for (var i = 0; i < pathElements.Length; i++)
			{
				if (currentElement == null || currentElement.OwnedElements == null)
					return null;
				currentElement =
					currentElement.OwnedElements
					.FirstOrDefault(e => e is INamedElement && ((INamedElement)e).Name.Equals(pathElements[i], StringComparison.InvariantCultureIgnoreCase)) as INamedElement;
			}

			return currentElement;
		}

		private INamedElement GetAggregateRootByFullName(IElement elt, string elementName)
		{
			INamedElement currentElement = GetRoot(elt);

			string[] pathElements = elementName.Split(new string[] { "::" }, StringSplitOptions.None);

			// The first path element will be the domain element so start searching for the second one within its owned elements
			for (int i = 0; i < pathElements.Length; i++)
			{
				if (currentElement == null || currentElement.OwnedElements == null)
					return null;
				currentElement =
					currentElement.OwnedElements
					.FirstOrDefault
					(e =>
						e is INamedElement
							&&
						((INamedElement)e).Name.Equals(pathElements[i], StringComparison.InvariantCultureIgnoreCase)
					) as INamedElement;
			}

			if (currentElement == null)
				return null;

			if (!currentElement.AppliedStereotypes.Any(property => property.Name == "AggregateRoot"))
				return null;

			return currentElement;
		}

		private string GetLowercaseFirstCharacter(string str)
		{
			if (string.IsNullOrWhiteSpace(str))
				return str;
			else
				return Char.ToLowerInvariant(str[0]) + str.Substring(1);
		}
		#endregion
	}
}