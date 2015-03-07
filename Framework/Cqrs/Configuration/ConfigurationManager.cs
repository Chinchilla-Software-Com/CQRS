#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion
namespace Cqrs.Configuration
{
	public class ConfigurationManager : IConfigurationManager
	{
		#region Implementation of IConfigurationManager

		public string GetSetting(string key)
		{
			return System.Configuration.ConfigurationManager.AppSettings[key];
		}

		#endregion
	}
}