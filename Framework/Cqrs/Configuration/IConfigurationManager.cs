#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Configuration
{
	/// <summary>
	/// Provides access to configuration settings.
	/// </summary>
	public interface IConfigurationManager
	{
		/// <summary>
		/// Read the setting named <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key (or name) of the setting to read.</param>
		string GetSetting(string key);

		/// <summary>
		/// Read the setting named <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key (or name) of the setting to read.</param>
		/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
		/// <returns>true if the an element with the specified key exists; otherwise, false.</returns>
		bool TryGetSetting(string key, out string value);

		/// <summary>
		/// Read the setting named <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key (or name) of the setting to read.</param>
		/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
		/// <returns>true if the an element with the specified key exists; otherwise, false.</returns>
		bool TryGetSetting(string key, out bool value);

		/// <summary>
		/// Read the configuration string named <paramref name="connectionStringName"/>.
		/// </summary>
		/// <param name="connectionStringName">The name (or key) of the connection string to read.</param>
		string GetConnectionString(string connectionStringName);

		/// <summary>
		/// Read the configuration string where the name (or key) of the connection string is stored in a setting, first obtained with a call to <see cref="GetSetting(string)"/>
		/// </summary>
		/// <param name="key">The key (or name) of the setting that has the name (or key) of the connection string to read.</param>
		string GetConnectionStringBySettingKey(string key);
	}
}