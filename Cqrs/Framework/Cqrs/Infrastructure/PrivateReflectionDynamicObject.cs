using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Cqrs.Infrastructure
{
	internal class PrivateReflectionDynamicObject : DynamicObject
	{
		public object RealObject { get; set; }
		private const System.Reflection.BindingFlags BindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

		internal static object WrapObjectIfNeeded(object @object)
		{
			// Don't wrap primitive types, which don't have many interesting internal APIs
			if (@object == null || @object.GetType().IsPrimitive || @object is string)
				return @object;

			return new PrivateReflectionDynamicObject { RealObject = @object };
		}

		// Called when a method is called
		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			result = InvokeMemberOnType(RealObject.GetType(), RealObject, binder.Name, args);

			// Wrap the sub object if necessary. This allows nested anonymous objects to work.
			result = WrapObjectIfNeeded(result);

			return true;
		}

		private static object InvokeMemberOnType(Type type, object target, string name, object[] args)
		{
			try
			{
				if (type.GetMember(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Any())
					return type.InvokeMember(name, System.Reflection.BindingFlags.InvokeMethod | BindingFlags, null, target, args);
				// If we couldn't find the method, try on the base class
				if (type.BaseType != null)
					return InvokeMemberOnType(type.BaseType, target, name, args);
			}
			catch (MissingMethodException)
			{
				// If we couldn't find the method, try on the base class
				if (type.BaseType != null)
					return InvokeMemberOnType(type.BaseType, target, name, args);
			}
				// Don't care if the method don't exist.
			return null;
		}
	}
}