#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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