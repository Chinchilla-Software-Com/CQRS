using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;

namespace Cqrs.Bus
{
	public class FileBasedLastEventProcessedStore : IStoreLastEventProcessed
	{
		public const string AppSettingsKey = "FileBasedLastEventProcessed.Location";

		public const string AppSettingsDefaultValue = @"%EVENTSTORE_HOME%\LastEventProcessedLocation";

		protected string FileName { get; private set; }

		public FileBasedLastEventProcessedStore()
		{
			NameValueCollection appSettings = ConfigurationManager.AppSettings;
			string location = appSettings[AppSettingsKey];
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
