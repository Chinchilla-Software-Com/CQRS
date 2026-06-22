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
	/// Provides a unified way of converting <see cref="string"/> values to other types with support for a textual "undefined" value referring to null.
	/// </summary>
	public class FromStringConverterAdapter
	{
		private readonly Type _type;

		private readonly TypeConverter _typeConverter;

		/// <summary>
		/// Instantiates a new instance of <see cref="FromStringConverterAdapter"/>.
		/// </summary>
		public FromStringConverterAdapter(Type type, TypeConverter typeConverter)
		{
			if(type == null)
				throw new ArgumentNullException("type");
			if (typeConverter == null)
				throw new ArgumentNullException("typeConverter");

			_type = type;
			_typeConverter = typeConverter;
		}

		/// <summary>
		/// Converts the given text to an object, using the specified context and culture information.
		/// </summary>
		/// <param name="culture">A <see cref="CultureInfo"/> that specifies the culture to which to convert.</param>
		/// <param name="value">The <see cref="string"/> to convert.</param>
		/// <returns>An <see cref="object"/> that represents the converted <paramref name="value"/>.</returns>
		public object ConvertFromString(string value, CultureInfo culture)
		{
			var isUndefinedNullable = Nullable.GetUnderlyingType(_type) != null && value == "undefined";
			if (isUndefinedNullable)
				return null;

			return _typeConverter.ConvertFromString(null, culture, value);
		}
	}
}