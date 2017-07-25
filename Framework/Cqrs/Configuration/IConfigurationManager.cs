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

		bool TryGetSetting(string key, out string value);

		bool TryGetSetting(string key, out bool value);
	}
}