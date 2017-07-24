#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.ComponentModel;
using System.Web.Mvc;

namespace Cqrs.Web.Mvc
{
	/// <summary>
	/// A <see cref="IModelBinder"/> designed to return <see cref="Guid.Empty"/> is not <see cref="Guid"/> is provided.
	/// </summary>
	public class NullableGuidBinder : DefaultModelBinder
	{
		/// <summary>
		/// Returns the value of a property using the specified <paramref name="controllerContext"/>, <paramref name="bindingContext"/>, <paramref name="propertyDescriptor"/>, and <paramref name="propertyBinder"/>.
		/// </summary>
		/// <param name="controllerContext">The context within which the controller operates. The context information includes the controller, HTTP content, request context, and route data.</param>
		/// <param name="bindingContext">The context within which the model is bound. The context includes information such as the model object, model name, model type, property filter, and value provider.</param>
		/// <param name="propertyDescriptor">The descriptor for the property to access. The descriptor provides information such as the component type, property type, and property value. It also provides methods to get or set the property value.</param>
		/// <param name="propertyBinder">An object that provides a way to bind the property.</param>
		/// <returns>An object that represents the property value.</returns>
		protected override object GetPropertyValue(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor, IModelBinder propertyBinder)
		{
			if (propertyDescriptor.PropertyType == typeof(Guid))
			{
				object rawValue = base.GetPropertyValue(controllerContext, bindingContext, propertyDescriptor, propertyBinder);
				if (rawValue == null)
					return Guid.Empty;
				return rawValue;
			}
			return base.GetPropertyValue(controllerContext, bindingContext, propertyDescriptor, propertyBinder);
		}
	}
}