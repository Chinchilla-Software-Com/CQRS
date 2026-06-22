#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Configuration;

namespace Cqrs.Exceptions
{
	/// <summary>
	/// An app setting is missing from <see cref="ConfigurationManager.AppSettings"/> that points to a connection string.
	/// </summary>
	[Serializable]
	public class MissingApplicationSettingForConnectionStringException : MissingApplicationSettingException
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="MissingApplicationSettingException"/>.
		/// </summary>
		/// <param name="appSettingKey">The key of the app setting that is missing.</param>
		public MissingApplicationSettingForConnectionStringException(string appSettingKey)
			: base(appSettingKey, true)
		{
		}

		/// <summary>
		/// Instantiate a new instance of <see cref="MissingApplicationSettingException"/> and a reference to the inner <paramref name="exception"/> that is the cause of this <see cref="Exception"/>.
		/// </summary>
		/// <param name="appSettingKey">The key of the app setting that is missing.</param>
		/// <param name="exception">The <see cref="Exception"/> that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner <see cref="Exception"/> is specified.</param>
		public MissingApplicationSettingForConnectionStringException(string appSettingKey, Exception exception)
			: base(appSettingKey, true, exception)
		{
		}
	}
}