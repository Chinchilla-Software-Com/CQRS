#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.ComponentModel;
using System.Web.Mvc;

namespace Cqrs.Web.Mvc
{
	public class NullableGuidBinder : DefaultModelBinder
	{
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