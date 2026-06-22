#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.WebApi.Formatters.FormMultipart.Infrastructure
{
	/// <summary>
	/// Represents a file.
	/// </summary>
	public class HttpFile
	{
		/// <summary>
		/// The name of the file.
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// The media type of the file.
		/// </summary>
		public string MediaType { get; set; }

		/// <summary>
		/// The content of the file
		/// </summary>
		public byte[] Buffer { get; set; }

		/// <summary>
		/// Instantiate and initialise a new instance of <see cref="HttpFile"/>
		/// </summary>
		public HttpFile() { }

		/// <summary>
		/// Instantiate and initialise a new instance of <see cref="HttpFile"/>
		/// </summary>
		public HttpFile(string fileName, string mediaType, byte[] buffer)
		{
			FileName = fileName;
			MediaType = mediaType;
			Buffer = buffer;
		}
	}
}