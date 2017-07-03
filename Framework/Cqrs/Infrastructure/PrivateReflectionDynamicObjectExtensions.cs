#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Infrastructure
{
	internal static class PrivateReflectionDynamicObjectExtensions
	{
		public static dynamic AsDynamic(this object @object)
		{
			return PrivateReflectionDynamicObject.WrapObjectIfNeeded(@object);
		}
	}
}