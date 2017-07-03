#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Configuration
{
	public class ConfigurationManager : IConfigurationManager
	{
		#region Implementation of IConfigurationManager

		public virtual string GetSetting(string key)
		{
			return System.Configuration.ConfigurationManager.AppSettings[key];
		}

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

		#endregion
	}
}