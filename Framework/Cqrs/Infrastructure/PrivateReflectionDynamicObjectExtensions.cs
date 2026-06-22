#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Dynamic;

namespace Cqrs.Infrastructure
{
	internal static class PrivateReflectionDynamicObjectExtensions
	{
		/// <summary>
		/// Convert the provided <paramref name="object"/> into a safe to use <see cref="DynamicObject"/>.
		/// </summary>
		public static dynamic AsDynamic(this object @object)
		{
			return PrivateReflectionDynamicObject.WrapObjectIfNeeded(@object);
		}
	}
}