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