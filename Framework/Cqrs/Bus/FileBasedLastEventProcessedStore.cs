#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.IO;
using Cqrs.Configuration;

namespace Cqrs.Bus
{
	/// <summary>
	/// Indicates the position in a store where the stream has been read up to by storing the value in a file.
	/// </summary>
	public class FileBasedLastEventProcessedStore : IStoreLastEventProcessed
	{
		/// <summary>
		/// The configuration setting that holds the location of file to store position information in.
		/// </summary>
		public const string AppSettingsKey = "Cqrs.FileBasedLastEventProcessed.Location";

		/// <summary>
		/// The default location of the file to store position information in.
		/// </summary>
		public const string AppSettingsDefaultValue = @"%EVENTSTORE_HOME%\LastEventProcessedLocation";

		/// <summary>
		/// The relative or absolute path of the file to store the current location in
		/// </summary>
		protected string FileName { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="FileBasedLastEventProcessedStore"/>.
		/// </summary>
		public FileBasedLastEventProcessedStore(IConfigurationManager configurationManager)
		{
			string location = configurationManager.GetSetting(AppSettingsKey);
			if (string.IsNullOrEmpty(location))
			{
				location = AppSettingsDefaultValue;
			}

			FileName = Environment.ExpandEnvironmentVariables(location);
		}

		/// <summary>
		/// Reads and writes the location within the store where the stream has been read up to <see cref="FileName"/>.
		/// </summary>
		public string EventLocation
		{
			get
			{
				return File.Exists(FileName) ? File.ReadAllText(FileName) : string.Empty;
			}

			set
			{
				File.WriteAllText(FileName, value);
			}
		}
	}
}
