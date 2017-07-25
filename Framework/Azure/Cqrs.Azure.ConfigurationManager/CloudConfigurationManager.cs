namespace Cqrs.Azure.ConfigurationManager
{
	/// <summary>
	/// Provides access to configuration settings from the Azure Portal application settings. If no value is stored there it falls back to reading 
	/// app settings of an app.config or web.config... i.e. <see cref="System.Configuration.ConfigurationManager.AppSettings"/>
	/// </summary>
	public class CloudConfigurationManager : Configuration.ConfigurationManager
	{
		#region Implementation of IConfigurationManager

		/// <summary>
		/// Read the setting named <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key (or name) of the setting to read.</param>
		public override string GetSetting(string key)
		{
#if DEBUG
			return Microsoft.Azure.CloudConfigurationManager.GetSetting(key, true);
#else
			return Microsoft.Azure.CloudConfigurationManager.GetSetting(key, false);
#endif
		}

		#endregion
	}
}
