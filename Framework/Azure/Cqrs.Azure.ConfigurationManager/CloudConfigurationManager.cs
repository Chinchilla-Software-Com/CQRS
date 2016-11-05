namespace Cqrs.Azure.ConfigurationManager
{
	public class CloudConfigurationManager : Configuration.ConfigurationManager
	{
		#region Implementation of IConfigurationManager

		public override string GetSetting(string key)
		{
#if DEBUG
			return Microsoft.Azure.CloudConfigurationManager.GetSetting(key, true);
#else
			return Microsoft.Azure.CloudConfigurationManager.GetSetting(key, false);
#endif
		}

		#endregion
	}
}
