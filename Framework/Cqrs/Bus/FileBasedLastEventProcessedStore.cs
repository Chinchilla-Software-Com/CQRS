#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.IO;
using Cqrs.Configuration;

namespace Cqrs.Bus
{
	public class FileBasedLastEventProcessedStore : IStoreLastEventProcessed
	{
		public const string AppSettingsKey = "FileBasedLastEventProcessed.Location";

		public const string AppSettingsDefaultValue = @"%EVENTSTORE_HOME%\LastEventProcessedLocation";

		protected string FileName { get; private set; }

		public FileBasedLastEventProcessedStore(IConfigurationManager configurationManager)
		{
			string location = configurationManager.GetSetting(AppSettingsKey);
			if (string.IsNullOrEmpty(location))
			{
				location = AppSettingsDefaultValue;
			}

			FileName = Environment.ExpandEnvironmentVariables(location);
		}

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
