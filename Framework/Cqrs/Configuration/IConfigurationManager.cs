#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Configuration
{
	public interface IConfigurationManager
	{
		string GetSetting(string key);

		bool TryGetSetting(string key, out string value);

		bool TryGetSetting(string key, out bool value);
	}
}