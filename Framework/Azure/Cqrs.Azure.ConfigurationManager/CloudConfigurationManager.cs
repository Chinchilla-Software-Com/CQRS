#if NETCOREAPP3_0
using Microsoft.Extensions.Configuration;
#endif

namespace Cqrs.Azure.ConfigurationManager
{
	/// <summary>
	/// Provides access to configuration settings from the Azure Portal application settings. If no value is stored there it falls back to reading 
	/// app settings of an app.config or web.config... i.e. <see cref="System.Configuration.ConfigurationManager.AppSettings"/>
	/// </summary>
	public class CloudConfigurationManager : Configuration.ConfigurationManager
	{
#if NETCOREAPP3_0
		/// <summary>
		/// Instantiate a new instance of a <see cref="CloudConfigurationManager"/>
		/// </summary>
		public CloudConfigurationManager(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		/// <summary>
		/// The <see cref="IConfiguration"/> that can be use to get configuration settings
		/// </summary>
		protected IConfiguration Configuration { get; private set; }
#endif

		#region Implementation of IConfigurationManager

		/// <summary>
		/// Read the setting named <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key (or name) of the setting to read.</param>
		public override string GetSetting(string key)
		{
#if NETCOREAPP3_0
			IConfigurationSection cqrsSection = Configuration.GetSection("Cqrs");
			return cqrsSection[key];
#endif
#if NET472
#if DEBUG
			return Microsoft.Azure.CloudConfigurationManager.GetSetting(key, true);
#else
			return Microsoft.Azure.CloudConfigurationManager.GetSetting(key, false);
#endif
#endif
		}

#if NETCOREAPP3_0
		/// <summary>
		/// Read the configuration string named <paramref name="connectionStringName"/>.
		/// </summary>
		/// <param name="connectionStringName">The name (or key) of the connection string to read.</param>
		public override string GetConnectionString(string connectionStringName)
		{
			return Configuration.GetConnectionString(connectionStringName);
		}
#endif

		#endregion
	}
}