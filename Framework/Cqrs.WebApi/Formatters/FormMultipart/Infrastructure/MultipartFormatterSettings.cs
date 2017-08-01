#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Globalization;

namespace Cqrs.WebApi.Formatters.FormMultipart.Infrastructure
{
	/// <summary>
	/// Settings for use during multi-part form-data formatting.
	/// </summary>
	public class MultipartFormatterSettings
	{
		/// <summary>
		/// Serialize byte array property as HttpFile when sending data if true or as indexed array if false
		/// (default value is "false)
		/// </summary>
		public bool SerializeByteArrayAsHttpFile { get; set; }

		/// <summary>
		/// Add validation error "The value is required." if no value is present in request for non-nullable property if this parameter is "true"
		/// (default value is "false)
		/// </summary>
		public bool ValidateNonNullableMissedProperty { get; set; }

		private CultureInfo _cultureInfo;

		/// <summary>
		/// Default is <see cref="System.Globalization.CultureInfo.CurrentCulture"/>
		/// </summary>
		public CultureInfo CultureInfo
		{
			get { return _cultureInfo ?? CultureInfo.CurrentCulture; }
			set { _cultureInfo = value; }
		}
	}
}
