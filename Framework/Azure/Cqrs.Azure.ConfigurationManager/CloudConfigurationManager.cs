using Cqrs.Configuration;

namespace Cqrs.Azure.ConfigurationManager
{
	public class CloudConfigurationManager : IConfigurationManager
	{
		#region Implementation of IConfigurationManager

		public string GetSetting(string key)
		{
			return Microsoft.WindowsAzure.CloudConfigurationManager.GetSetting(key);
		}

		#endregion
	}
}
