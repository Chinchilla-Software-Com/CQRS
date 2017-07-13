#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.ComponentModel;
using System.Globalization;

namespace Cqrs.WebApi.Formatters.FormMultipart.Infrastructure.TypeConverters
{
	public class BooleanConverterEx : BooleanConverter
	{
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value != null)
			{
				var str = value.ToString();

				if (String.Compare(str, "on", culture, CompareOptions.IgnoreCase) == 0)
					return true;

				if (String.Compare(str, "off", culture, CompareOptions.IgnoreCase) == 0)
					return false;
			}

			return base.ConvertFrom(context, culture, value);
		}
	}
}
