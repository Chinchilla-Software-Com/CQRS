#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Configuration;
using System.Runtime.Serialization;

namespace Cqrs.Exceptions
{
	/// <summary>
	/// An app setting is missing from <see cref="ConfigurationManager.AppSettings"/>.
	/// </summary>
	[Serializable]
	public class MissingApplicationSettingException : Exception
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="MissingApplicationSettingException"/>.
		/// </summary>
		/// <param name="appSettingKey">The key of the app setting that is missing.</param>
		public MissingApplicationSettingException(string appSettingKey)
			: this(appSettingKey, false)
		{
		}

		/// <summary>
		/// Instantiate a new instance of <see cref="MissingApplicationSettingException"/> with a specified error message.
		/// </summary>
		/// <param name="appSettingKey">The key of the app setting that is missing.</param>
		/// <param name="message">The message that describes the error.</param>
		public MissingApplicationSettingException(string appSettingKey, string message)
			: base(message)
		{
			AppSettingKey = appSettingKey;
		}

		/// <summary>
		/// Instantiate a new instance of <see cref="MissingApplicationSettingException"/> with a specified error message and  and a reference to the inner <paramref name="exception"/> that is the cause of this <see cref="Exception"/>
		/// </summary>
		/// <param name="appSettingKey">The key of the app setting that is missing.</param>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="exception">The <see cref="Exception"/> that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner <see cref="Exception"/> is specified.</param>
		public MissingApplicationSettingException(string appSettingKey, string message, Exception exception)
			: base(message, exception)
		{
			AppSettingKey = appSettingKey;
		}

		/// <summary>
		/// Instantiate a new instance of <see cref="MissingApplicationSettingException"/>.
		/// </summary>
		/// <param name="appSettingKey">The key of the app setting that is missing.</param>
		/// <param name="settingLocatesConnectionString">If true, this missing app setting points to a connection string.</param>
		protected MissingApplicationSettingException(string appSettingKey, bool settingLocatesConnectionString)
			: base(string.Format(settingLocatesConnectionString ? "No app setting with the key '{0}' was found in the configuration file with the name of a connection string to look for." : "No app setting with the key '{0}' was found in the configuration file.", appSettingKey))
		{
			AppSettingKey = appSettingKey;
		}

		/// <summary>
		/// Instantiate a new instance of <see cref="MissingApplicationSettingException"/> and a reference to the inner <paramref name="exception"/> that is the cause of this <see cref="Exception"/>.
		/// </summary>
		/// <param name="appSettingKey">The key of the app setting that is missing.</param>
		/// <param name="exception">The <see cref="Exception"/> that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner <see cref="Exception"/> is specified.</param>
		public MissingApplicationSettingException(string appSettingKey, Exception exception)
			: this(appSettingKey, false, exception)
		{
		}

		/// <summary>
		/// Instantiate a new instance of <see cref="MissingApplicationSettingException"/> and a reference to the inner <paramref name="exception"/> that is the cause of this <see cref="Exception"/>.
		/// </summary>
		/// <param name="appSettingKey">The key of the app setting that is missing.</param>
		/// <param name="settingLocatesConnectionString">If true, this missing app setting points to a connection string.</param>
		/// <param name="exception">The <see cref="Exception"/> that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner <see cref="Exception"/> is specified.</param>
		protected MissingApplicationSettingException(string appSettingKey, bool settingLocatesConnectionString, Exception exception)
			: base(string.Format(settingLocatesConnectionString ? "No app setting with the key '{0}' was found in the configuration file with the name of a connection string to look for." : "No app setting with the key '{0}' was found in the configuration file.", appSettingKey), exception)
		{
			AppSettingKey = appSettingKey;
		}

		/// <summary>
		/// The key of the app setting that is missing.
		/// </summary>
		[DataMember]
		public string AppSettingKey { get; set; }
	}
}