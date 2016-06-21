#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
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