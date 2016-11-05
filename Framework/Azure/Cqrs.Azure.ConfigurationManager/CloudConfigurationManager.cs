namespace Cqrs.Azure.ConfigurationManager
{
	public class CloudConfigurationManager : Configuration.ConfigurationManager
	{
		#region Implementation of IConfigurationManager

		public override string GetSetting(string key)
		{
			return Microsoft.Azure.CloudConfigurationManager.GetSetting(key, false);
		}

		#endregion
	}
}
