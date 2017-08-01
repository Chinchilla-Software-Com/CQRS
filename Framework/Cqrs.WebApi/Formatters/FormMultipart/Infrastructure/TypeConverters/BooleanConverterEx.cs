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
	/// <summary>
	/// Provides a <see cref="TypeConverter"/> to convert <see cref="bool"/> objects to and from various other representations
	/// with support for HTTP specification specific values such as ON or OFF.
	/// </summary>
	public class BooleanConverterEx : BooleanConverter
	{
		/// <summary>
		/// Converts the given <paramref name="value"/> object to a <see cref="bool"/> object.
		/// </summary>
		/// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="culture">A <see cref="CultureInfo"/> that specifies the culture to which to convert.</param>
		/// <param name="value">The <see cref="object"/> to convert.</param>
		/// <returns>An <see cref="object"/> that represents the converted <paramref name="value"/>.</returns>
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