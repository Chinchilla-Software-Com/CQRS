#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Exceptions;

namespace Cqrs.Configuration
{
	/// <summary>
	/// Provides access to configuration settings from the app settings of an app.config or web.config... i.e. <see cref="System.Configuration.ConfigurationManager.AppSettings"/>
	/// </summary>
	public class ConfigurationManager : IConfigurationManager
	{
#if NETSTANDARD2_0 || NETCOREAPP3_1_OR_GREATER
		/// <summary>
		/// Gets or sets the <see cref="Microsoft.Extensions.Configuration.IConfiguration"/>. This must be set manually as dependency injection may not be ready in-time.
		/// </summary>
		public static Microsoft.Extensions.Configuration.IConfiguration Configuration { get; set; }
#endif

		#region Implementation of IConfigurationManager

		/// <summary>
		/// Read the setting named <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key (or name) of the setting to read.</param>
		public virtual string GetSetting(string key)
		{
			return System.Configuration.ConfigurationManager.AppSettings[key];
		}

		/// <summary>
		/// Read the setting named <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key (or name) of the setting to read.</param>
		/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
		/// <returns>true if the an element with the specified key exists; otherwise, false.</returns>
		public virtual bool TryGetSetting(string key, out string value)
		{
			try
			{
				value = GetSetting(key);
				return true;
			}
			catch (Exception)
			{
				value = null;
				return false;
			}
		}

		/// <summary>
		/// Read the setting named <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key (or name) of the setting to read.</param>
		/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
		/// <returns>true if the an element with the specified key exists; otherwise, false.</returns>
		public virtual bool TryGetSetting(string key, out bool value)
		{
			string rawValue;
			if (TryGetSetting(key, out rawValue))
			{
				if (bool.TryParse(rawValue, out value))
				{
					return true;
				}
			}
			value = false;
			return false;
		}

		/// <summary>
		/// Read the configuration string named <paramref name="connectionStringName"/>.
		/// </summary>
		/// <param name="connectionStringName">The name (or key) of the connection string to read.</param>
		public virtual string GetConnectionString(string connectionStringName)
		{
			return System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
		}

		/// <summary>
		/// Read the configuration string where the name (or key) of the connection string is stored in a setting, first obtained with a call to <see cref="GetSetting(string)"/>
		/// </summary>
		/// <param name="key">The key (or name) of the setting that has the name (or key) of the connection string to read.</param>
		/// <param name="throwIfKeyMissing">If true, will throw a <see cref="MissingApplicationSettingForConnectionStringException"/> if no application setting with the provided <paramref name="key"/> is found.</param>
		/// <param name="throwIfConnectionstringMissing">If true, will throw a <see cref="MissingConnectionStringException"/> if no connection string is found.</param>
		public virtual string GetConnectionStringBySettingKey(string key, bool throwIfKeyMissing = false, bool throwIfConnectionstringMissing = false)
		{
			if (!TryGetSetting(key, out string applicationKey) || string.IsNullOrEmpty(applicationKey))
				throw new MissingApplicationSettingForConnectionStringException(key);
			string connectionStringKey = GetConnectionString(applicationKey);
			if (string.IsNullOrWhiteSpace(connectionStringKey))
				throw new MissingConnectionStringException(applicationKey);
			return connectionStringKey;
		}

		#endregion
	}
}