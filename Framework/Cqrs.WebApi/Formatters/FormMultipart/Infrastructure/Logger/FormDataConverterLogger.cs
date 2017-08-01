#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cqrs.WebApi.Formatters.FormMultipart.Infrastructure.Logger
{
	/// <summary>
	/// Provide a mechanism to log issues and <see cref="Exception"/> data during conversions.
	/// </summary>
	public class FormDataConverterLogger : IFormDataConverterLogger
	{
		private Dictionary<string, List<LogErrorInfo>> Errors { get; set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="FormDataConverterLogger"/>.
		/// </summary>
		public FormDataConverterLogger()
		{
			Errors = new Dictionary<string, List<LogErrorInfo>>();
		}

		/// <summary>
		/// Logs an error.
		/// </summary>
		/// <param name="errorPath">The path to the member for which the error is being logged.</param>
		/// <param name="exception">The exception to be logged.</param>
		public void LogError(string errorPath, Exception exception)
		{
			AddError(errorPath, new LogErrorInfo(exception));
		}

		/// <summary>
		/// Logs an error.
		/// </summary>
		/// <param name="errorPath">The path to the member for which the error is being logged.</param>
		/// <param name="errorMessage">The error message to be logged.</param>
		public void LogError(string errorPath, string errorMessage)
		{
			AddError(errorPath, new LogErrorInfo(errorMessage));
		}
		
		/// <summary>
		/// Get all errors recorded.
		/// </summary>
		public List<LogItem> GetErrors()
		{
			return Errors.Select
			(
				m => new LogItem
				{
					ErrorPath = m.Key,
					Errors = m.Value.Select(t => t).ToList()
				}
			).ToList();
		}

		/// <summary>
		/// Throw exception if errors found
		/// </summary>
		public void EnsureNoErrors()
		{
			if (Errors.Any())
			{
				var errors = Errors
					.Select(m => String.Format("{0}: {1}", m.Key, String.Join(". ", m.Value.Select(x => (x.ErrorMessage ?? (x.Exception != null ? x.Exception.Message : ""))))))
					.ToList();

				string errorMessage = String.Join(" ", errors);

				throw new Exception(errorMessage);
			}
		}

		private void AddError(string errorPath, LogErrorInfo info)
		{
			List<LogErrorInfo> listErrors;
			if (!Errors.TryGetValue(errorPath, out listErrors))
			{
				listErrors = new List<LogErrorInfo>();
				Errors.Add(errorPath, listErrors);
			}
			listErrors.Add(info);
		}

		/// <summary>
		/// Errors for a given path.
		/// </summary>
		public class LogItem
		{
			/// <summary>
			/// The path identifying where the <see cref="Exception"/> or issue occurred.
			/// </summary>
			public string ErrorPath { get; set; }

			/// <summary>
			/// All <see cref="Exception">exceptions</see> or issues that occurred for the <see cref="ErrorPath"/>.
			/// </summary>
			public List<LogErrorInfo> Errors { get; set; }
		}

		/// <summary>
		/// An error, issue or <see cref="Exception"/>.
		/// </summary>
		public class LogErrorInfo
		{
			/// <summary>
			/// The details of the error, issue or <see cref="Exception"/>.
			/// </summary>
			public string ErrorMessage { get; private set; }

			/// <summary>
			/// The <see cref="Exception"/> if <see cref="IsException"/> is true.
			/// </summary>
			public Exception Exception { get; private set; }

			/// <summary>
			/// Indicates of the error or issue was an exception.
			/// </summary>
			public bool IsException { get; private set; }

			/// <summary>
			/// Instantiates a new instance of <see cref="FormDataConverterLogger"/> with the specified <paramref name="errorMessage"/>.
			/// </summary>
			public LogErrorInfo(string errorMessage)
			{
				ErrorMessage = errorMessage;
				IsException = false;
			}

			/// <summary>
			/// Instantiates a new instance of <see cref="FormDataConverterLogger"/> with the specified <paramref name="exception"/>
			/// </summary>
			public LogErrorInfo(Exception exception)
			{
				Exception = exception;
				IsException = true;
			}
		}
	}
}