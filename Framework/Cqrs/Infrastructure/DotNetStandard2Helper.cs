#if NETSTANDARD2_0 || NETCOREAPP3_1_OR_GREATER
using System;
using System.Globalization;
using System.Reflection;

/// <remarks>
/// Taken from https://github.com/dotnet/corefx/blob/5648632fd6f974e2250c1435caeeed58a3acb4f3/src/Common/src/CoreLib/System/Activator.cs
/// </remarks>
internal static class DotNetStandard2Helper
{
	/// <summary>
	/// Creates an instance of the type whose name is specified, using the named assembly file and parameterless constructor.
	/// </summary>
	/// <param name="assemblyFile">The name of a file that contains an assembly where the type named typeName is sought.</param>
	/// <param name="typeName">The name of the preferred type.</param>
	/// <returns>A handle that must be unwrapped to access the newly created instance.</returns>
	/// <exception cref="System.ArgumentNullException">typeName is null.</exception>
	/// <exception cref="System.MissingMethodException">No matching public constructor was found.</exception>
	/// <exception cref="System.TypeLoadException">typename was not found in assemblyFile.</exception>
	/// <exception cref="System.IO.FileNotFoundException">assemblyFile was not found.</exception>
	/// <exception cref="System.MethodAccessException">The caller does not have permission to call this constructor.</exception>
	/// <exception cref="System.MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism.</exception>
	/// <exception cref="System.Reflection.TargetInvocationException">The constructor, which was invoked through reflection, threw an exception.</exception>
	/// <exception cref="System.Security.SecurityException">The caller does have the required System.Security.Permissions.FileIOPermission.</exception>
	/// <exception cref="System.BadImageFormatException">assemblyFile is not a valid assembly. -or- The common language runtime (CLR) version 2.0 or later is currently loaded, and assemblyName was compiled for a version of the CLR that is later than the currently loaded version. Note that the .NET Framework versions 2.0, 3.0, and 3.5 all use CLR version 2.0.</exception>
	internal static object CreateInstanceFrom(string assemblyFile, string typeName)
	{
		Assembly assembly = Assembly.LoadFrom(assemblyFile);
		Type t = assembly.GetType(typeName);

		return Activator.CreateInstance(t);
	}

	internal static object CreateInstance(string assemblyName, string typeName)
	{
		Type type = null;
		Assembly assembly = null;
		if (assemblyName == null)
			assembly = Assembly.GetExecutingAssembly();
		else
		{
			AssemblyName _assemblyName = new AssemblyName(assemblyName);

			if (_assemblyName.ContentType == AssemblyContentType.WindowsRuntime)
				// WinRT type - we have to use Type.GetType
				type = Type.GetType(typeName + ", " + assemblyName, throwOnError: true, false);
			else
				// Classic managed type
				assembly = AppDomain.CurrentDomain.Load(assemblyName);
		}

		if (type == null)
			type = assembly.GetType(typeName, throwOnError: true, false);

		return Activator.CreateInstance(type);
	}

	internal static object CreateInstanceFrom(string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
	{
		Assembly assembly = Assembly.LoadFrom(assemblyFile);
		Type t = assembly.GetType(typeName, true, ignoreCase);

		return Activator.CreateInstance(t, bindingAttr, binder, args, culture, activationAttributes);
	}
}
#endif