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
	public class FromStringConverterAdapter
	{
		private readonly Type _type;

		private readonly TypeConverter _typeConverter;

		public FromStringConverterAdapter(Type type, TypeConverter typeConverter)
		{
			if(type == null)
				throw new ArgumentNullException("type");
			if (typeConverter == null)
				throw new ArgumentNullException("typeConverter");

			_type = type;
			_typeConverter = typeConverter;
		}

		public object ConvertFromString(string src, CultureInfo culture)
		{
			var isUndefinedNullable = Nullable.GetUnderlyingType(_type) != null && src == "undefined";
			if (isUndefinedNullable)
				return null;

			return _typeConverter.ConvertFromString(null, culture, src);
		}
	}
}
