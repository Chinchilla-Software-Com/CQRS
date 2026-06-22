#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Net.Http.Formatting;

namespace Cqrs.WebApi.Formatters.FormMultipart.Infrastructure.Logger
{
	/// <summary>
	/// Provide a mechanism to log issues and <see cref="Exception"/> data during conversions via a <see cref="IFormatterLogger"/>.
	/// </summary>
	internal class FormatterLoggerAdapter : IFormDataConverterLogger
	{
		private IFormatterLogger FormatterLogger { get; set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="FormatterLoggerAdapter"/>.
		/// </summary>
		public FormatterLoggerAdapter(IFormatterLogger formatterLogger)
		{
			if(formatterLogger == null)
				throw new ArgumentNullException("formatterLogger");
			FormatterLogger = formatterLogger;
		}

		/// <summary>
		/// Logs an error.
		/// </summary>
		/// <param name="errorPath">The path to the member for which the error is being logged.</param>
		/// <param name="exception">The exception to be logged.</param>
		public void LogError(string errorPath, Exception exception)
		{
			FormatterLogger.LogError(errorPath, exception);
		}

		/// <summary>
		/// Logs an error.
		/// </summary>
		/// <param name="errorPath">The path to the member for which the error is being logged.</param>
		/// <param name="errorMessage">The error message to be logged.</param>
		public void LogError(string errorPath, string errorMessage)
		{
			FormatterLogger.LogError(errorPath, errorMessage);
		}

		/// <summary>
		/// Does nothing.
		/// </summary>
		public void EnsureNoErrors() 
		{
			//nothing to do
		}
	}
}
