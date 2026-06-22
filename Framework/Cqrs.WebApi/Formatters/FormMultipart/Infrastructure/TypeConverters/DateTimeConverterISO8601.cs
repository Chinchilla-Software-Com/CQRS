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
	/// Convert <see cref="DateTime"/> to ISO 8601 format string
	/// </summary>
	internal class DateTimeConverterIso8601 : DateTimeConverter
	{
		/// <summary>
		/// Converts the given <paramref name="value"/> object to a <see cref="DateTime"/> using the arguments.
		/// </summary>
		/// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="culture">A optional <see cref="CultureInfo"/>. If not supplied, the current culture is assumed.</param>
		/// <param name="value">The <see cref="object"/> to convert.</param>
		/// <param name="destinationType">The <see cref="Type"/> to convert the <paramref name="value"/> to.</param>
		/// <returns>An <see cref="object"/> that represents the converted <paramref name="value"/>.</returns>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value != null && value is DateTime && destinationType == typeof (string))
			{
				return ((DateTime)value).ToString("O"); // ISO 8601
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
