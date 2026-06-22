#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Cqrs.WebApi.Formatters.FormMultipart.Converters;
using Cqrs.WebApi.Formatters.FormMultipart.Infrastructure;
using Cqrs.WebApi.Formatters.FormMultipart.Infrastructure.Logger;

namespace Cqrs.WebApi.Formatters.FormMultipart
{
	/// <summary>
	/// Represents the <see cref="MediaTypeFormatter"/> class to handle multi-part form-data.
	/// </summary>
	public class FormMultipartEncodedMediaTypeFormatter : MediaTypeFormatter
	{
		private const string SupportedMediaType = "multipart/form-data";
		
		private readonly MultipartFormatterSettings Settings;

		/// <summary>
		/// Instantiate and initialise a new instance of <see cref="FormMultipartEncodedMediaTypeFormatter"/>
		/// </summary>
		/// <param name="settings">The <see cref="MultipartFormatterSettings"/> to use.</param>
		public FormMultipartEncodedMediaTypeFormatter(MultipartFormatterSettings settings = null)
		{
			Settings = settings ?? new MultipartFormatterSettings();
			SupportedMediaTypes.Add(new MediaTypeHeaderValue(SupportedMediaType));
		}

		/// <summary>
		/// Queries whether this <see cref="MediaTypeFormatter"/> can deserialise an object of the specified type.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> to deserialise.</param>
		/// <returns>true if the <see cref="MediaTypeFormatter"/> can deserialise the <paramref name="type"/>; otherwise, false.</returns>
		public override bool CanReadType(Type type)
		{
			return true;
		}

		/// <summary>
		/// Queries whether this <see cref="MediaTypeFormatter"/> can serialise an object of the specified type.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> to serialise.</param>
		/// <returns>true if the <see cref="MediaTypeFormatter"/> can serialise the <paramref name="type"/>; otherwise, false.</returns>
		public override bool CanWriteType(Type type)
		{
			return true;
		}

		/// <summary>
		/// Sets the default headers for content that will be formatted using this formatter.
		/// This method is called from the <see cref="ObjectContent"/> constructor..
		/// This implementation sets the Content-Type header to the value of <paramref name="mediaType"/> if it is not null.
		/// If it is null it sets the Content-Type to the default media type of this formatter.
		/// If the Content-Type does not specify a charset it will set it using this formatters configured <see cref="Encoding"/>.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> of the object being serialized. See <see cref="ObjectContent"/>.</param>
		/// <param name="headers">The content headers that should be configured.</param>
		/// <param name="mediaType">The authoritative media type. Can be null.</param>
		public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
		{
			base.SetDefaultContentHeaders(type, headers, mediaType);

			//need add boundary
			//(if add when fill SupportedMediaTypes collection in class constructor then receive post with another boundary will not work - Unsupported Media Type exception will thrown)
			if (headers.ContentType == null)
			{
				headers.ContentType = new MediaTypeHeaderValue(SupportedMediaType);
			}
			if (!String.Equals(headers.ContentType.MediaType, SupportedMediaType, StringComparison.OrdinalIgnoreCase))
			{
				throw new Exception("Not a Multipart Content");
			}
			if (headers.ContentType.Parameters.All(m => m.Name != "boundary"))
			{
				headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", "Cqrs.WebApi.Formatters.FormMultipartBoundary1q2w3e"));
			}
		}

		/// <summary>
		/// Asynchronously deserialises an object of the specified type.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> of the object to deserialise.</param>
		/// <param name="readStream">The <see cref="Stream"/> to read.</param>
		/// <param name="content">The <see cref="HttpContent"/>, if available. It may be null.</param>
		/// <param name="formatterLogger">The <see cref="IFormatterLogger"/> to log events to.</param>
		/// <returns>A <see cref="Task"/> whose result will be an object of the given type.</returns>
		public override async Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
		{
			var httpContentToFormDataConverter = new HttpContentToFormDataConverter();
			FormData multipartFormData = await httpContentToFormDataConverter.Convert(content);

			IFormDataConverterLogger logger;
			if (formatterLogger != null)
				logger = new FormatterLoggerAdapter(formatterLogger);
			else 
				logger = new FormDataConverterLogger();

			var dataToObjectConverter = new FormDataToObjectConverter(multipartFormData, logger, Settings);
			object result = dataToObjectConverter.Convert(type);

			logger.EnsureNoErrors();

			return result;
		}

		/// <summary>
		/// Asynchronously writes an object of the specified type.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> of the object to write.</param>
		/// <param name="value">The object value to write. It may be null.</param>
		/// <param name="writeStream">The <see cref="Stream"/> to write to.</param>
		/// <param name="content">The <see cref="HttpContent"/>, if available. It may be null.</param>
		/// <param name="transportContext">The <see cref="TransportContext"/> if available. It may be null.</param>
		/// <returns>A <see cref="Task"/> that will perform the write.</returns>
		public override async Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
		{
			if (!content.IsMimeMultipartContent())
			{
				throw new Exception("Not a Multipart Content");
			}

			var boudaryParameter = content.Headers.ContentType.Parameters.FirstOrDefault(m => m.Name == "boundary" && !String.IsNullOrWhiteSpace(m.Value));
			if (boudaryParameter == null)
			{
				throw new Exception("multipart boundary not found");
			}

			var objectToMultipartDataByteArrayConverter = new ObjectToMultipartDataByteArrayConverter(Settings);
			byte[] multipartData = objectToMultipartDataByteArrayConverter.Convert(value, boudaryParameter.Value);

			await writeStream.WriteAsync(multipartData, 0, multipartData.Length);

			content.Headers.ContentLength = multipartData.Length;
		}
	}
}