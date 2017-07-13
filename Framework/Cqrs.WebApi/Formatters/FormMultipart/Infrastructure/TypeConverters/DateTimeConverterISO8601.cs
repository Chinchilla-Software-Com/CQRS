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
